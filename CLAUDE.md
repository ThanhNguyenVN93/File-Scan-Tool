# File Scan Tool — Spec

> **Xem `PROGRESS.md` trước** để biết trạng thái implementation hiện tại (đã làm gì, còn gì chưa
> chắc chắn cần user xác nhận) — file này chỉ là spec gốc, không phản ánh các quyết định phát
> sinh trong quá trình làm.

## Mục tiêu
Desktop tool cho Windows, cho phép người dùng upload file nghi ngờ để scan virus qua nhiều nguồn online (VirusTotal, MetaDefender, Kaspersky OpenTIP) mà không cần mở trình duyệt. Kết quả (sạch/dính virus, hash) hiển thị ngay trong tool.

## Tech stack
- **.NET Framework 4.7.2 trở lên** (target build: 4.7.2, chạy tốt trên máy đã có 4.8 vì .NET 4.x là in-place upgrade). Bắt buộc để hỗ trợ Windows 7 SP1 trở lên — không dùng .NET 8 vì không hỗ trợ Win7.
- WinForms + Guna.UI2.WinForms (rounded corner, dark theme, animation xổ xuống)
- `HttpClient` cho gọi API bất đồng bộ
- Newtonsoft.Json (hoặc System.Text.Json nếu tương thích 4.7.2) để parse JSON response
- Từ 4.7.2 trở lên, TLS 1.2 là mặc định — không cần set `ServicePointManager.SecurityProtocol` thủ công (nếu hạ xuống 4.6 thì bắt buộc phải set `ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;` ở đầu chương trình vì cả 3 nguồn scan đều yêu cầu TLS 1.2)

## Nguồn scan (đã xác nhận có API free)
1. **VirusTotal** — API công khai, cần API key free, giới hạn ~4 req/phút, file tối đa 32MB. Endpoint upload + poll bằng `analysis_id`.
2. **MetaDefender Cloud (OPSWAT)** — `https://api.metadefender.com/v4/file`, cần API key free (đăng ký tài khoản OPSWAT), giới hạn request/ngày. Upload trả về `data_id`, poll lấy kết quả.
3. **Kaspersky OpenTIP** — `https://opentip.kaspersky.com/api/v1/...`, cần token từ tài khoản Kaspersky. Hỗ trợ tra hash và upload file.

~~Jotti's malware scan~~ — đã loại bỏ, không có API free tự đăng ký (chỉ có gói doanh nghiệp trả phí).

## Luồng xử lý (flow)
1. Người dùng chọn file (browse hoặc nhập path)
2. Tool tính hash SHA-256 (và MD5) của file local, hiển thị ngay
3. Người dùng chọn **1 nguồn** (dropdown: VirusTotal / MetaDefender / OpenTIP) rồi bấm Scan
4. Tool tra cứu hash trên nguồn đã chọn trước — nếu đã có kết quả cũ thì hiển thị ngay, không cần upload file thật
5. Nếu hash chưa có kết quả, tool upload file lên nguồn đó, hiển thị progress bar theo % tiến trình, rồi poll đến khi có kết quả
6. Kết quả hiển thị trong bảng xổ xuống bên dưới thanh, gồm 2 tab: **Dính virus** (kèm tên từng engine phát hiện) và **Sạch** (tỷ lệ %), cùng hash SHA-256/MD5 ở cuối

**Lưu ý quan trọng**: Dropdown chọn nguồn = chọn 1 nguồn DUY NHẤT để quét trong lượt đó. Không quét tuần tự cả 3 nguồn cùng lúc, không có logic tự động fallback sang nguồn khác khi lỗi — nếu nguồn được chọn lỗi/hết quota thì báo lỗi trực tiếp cho người dùng biết.

## Kiến trúc code

### Model
```csharp
class ScanResult
{
    string SourceName;
    bool IsClean;
    string DetectionRatio; // vd "2/68"
    Dictionary<string, string> EngineResults; // tên engine -> verdict
    string Sha256;
    string Md5;
    string ErrorMessage; // null nếu thành công
}

class FileHashInfo
{
    string Md5;
    string Sha1;
    string Sha256;
}
```

### Scan providers (interface chung)
```csharp
interface IScanProvider
{
    string Name { get; }
    Task<ScanResult> LookupByHashAsync(string sha256, CancellationToken ct);
    Task<ScanResult> UploadAndScanAsync(string filePath, IProgress<int> progress, CancellationToken ct);
}
```
- `VirusTotalProvider`, `MetaDefenderProvider`, `OpenTipProvider` — mỗi class implement interface này, tự xử lý API key riêng và polling riêng.
- Dropdown UI chọn provider nào thì gọi thẳng provider đó — không cần orchestrator điều phối nhiều nguồn.

### UI layer
- `MainBar` — Form nhỏ, always-on-top, dạng thanh pill bo tròn tối màu: textbox path file, dropdown chọn nguồn, nút Scan
- `ResultPanel` — control xổ xuống bên dưới `MainBar` khi bấm Scan, gồm:
  - Label + progress bar tiến trình quét (theo % của nguồn đang chọn)
  - 2 tab: "Dính virus (N)" và "Sạch (N)"
  - Danh sách engine + verdict (tab Dính virus)
  - Hash SHA-256 và MD5 ở cuối panel

## Bootstrapper .NET Framework

Mỗi lần mở app, tự kiểm tra và cài .NET nếu thiếu, trước khi vào app chính.

### Flow
1. Khởi động app → đọc registry `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full`, giá trị `Release`
2. Nếu `Release >= 461808` (tương ứng .NET 4.7.2 trở lên, bao gồm cả máy đã có 4.8) → mở thẳng app chính
3. Nếu chưa đủ:
   - Yêu cầu quyền Admin (UAC) — app cần `requireAdministrator` trong manifest
   - Chạy bộ cài **NetFx48Installer** (đã có sẵn, offline) ở chế độ silent: `/quiet /norestart`
   - Nếu cài fail (thường gặp trên các bản Windows 7 bị strip/embedded thiếu thành phần hệ thống dùng chung, không phải do thiếu .NET 3.5): chạy bước tự sửa hệ thống — tái sử dụng cơ chế repair (reset Windows Update service, `sfc /scannow`, `DISM /RestoreHealth`) từ tool có sẵn `Dotnet-35-Install-And-Fix-Tool` (github.com/ThanhCode93x/Dotnet-35-Install-And-Fix-Tool) — rồi thử cài lại, tối đa 2 lần
   - Kiểm tra exit code: `0` = thành công, `3010` = thành công nhưng cần khởi động lại máy → nếu vậy, thông báo và yêu cầu người dùng restart trước khi dùng app
4. Sau khi đủ .NET (ngay từ đầu hoặc sau khi cài) → mở app chính

**Lưu ý quan trọng**: File Scan Tool này **không phụ thuộc .NET 3.5** — trên Windows 7 SP1, .NET 3.5 (CLR 2.0) và .NET 4.x (CLR 4.0) là 2 runtime độc lập, không cần bật .NET 3.5 để chạy app 4.x. Tool `Dotnet-35-Install-And-Fix-Tool` chỉ được tái sử dụng ở đây cho phần cơ chế repair hệ thống (sfc/DISM), không phải để cài .NET 3.5.

### Tài nguyên có sẵn
- `NetFx48Installer` — bộ cài offline .NET Framework 4.8
- `Dotnet-35-Install-And-Fix-Tool` (repo GitHub riêng) — dùng lại phần Error Auto-Fix (đọc mã lỗi DISM, sửa Windows Update service, sfc/DISM repair)

## UI reference
Thanh chính: nền tối `#2b2a28`, bo tròn pill (`border-radius: 999px`), icon shield bên trái, input path (readonly, chỉ hiển thị), dropdown nguồn, nút Scan màu amber (`#f2a623`).

Panel kết quả: nền sáng theo theme app, bo góc 12px, border mỏng. Tab "Dính virus" active mặc định nếu có kết quả dương tính (chữ đỏ), tab "Sạch" (chữ xám). Hash hiển thị font mono, cỡ nhỏ, màu xám.
