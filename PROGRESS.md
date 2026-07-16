# File Scan Tool - Implementation Progress

> File nay ghi lai toan bo trang thai du an de phien Claude Code moi (chat moi) doc va tiep tuc
> ma khong can nguoi dung ke lai tu dau. Xem `CLAUDE.md` cho spec goc. Cap nhat file nay sau moi
> buoc lon hoac quyet dinh quan trong.

## Trang thai tong quan

Toan bo 6 buoc goc trong `CLAUDE.md` da xong (Model -> Provider x3 -> UI -> Bootstrapper), cong
them Settings UI (BYOK), theme polish, ~20 tinh nang mo rong vong 1-2, va 4 tinh nang vong 3 (Scan
URL, Batch hash lookup, Proxy config, Quota display). Build sach (`dotnet build
frm_scan_online.csproj`), da smoke-test nhieu lan bang PowerShell + screenshot that (khong chi doc
code). VirusTotal va OpenTIP da duoc test bang key that va xac nhan hoat dong (ca scan file lan
scan URL/batch lookup/quota). **MetaDefender chua duoc test upload/scan that** (chi verify qua
code review + doc chinh thuc); rieng MetaDefender URL scan **chu dong khong lam** (xem vong 3 ben
duoi).

## Kien truc

```
Models/       ScanResult (co TargetUrl cho ket qua scan URL), FileHashInfo, ScanHistoryEntry
Providers/    IScanProvider (LookupByHashAsync, UploadAndScanAsync, ScanUrlAsync, GetQuotaAsync) +
              VirusTotalProvider, MetaDefenderProvider, OpenTipProvider, MissingApiKeyException,
              Http/ProgressStreamContent (dung chung cho upload progress)
Services/     FileHasher, SettingsStore (DPAPI-encrypted API key + proxy settings storage),
              HttpClientFactory (ap dung proxy System/Manual/None cho ca 3 provider), QuotaInfo,
              StartupRegistration, BrowserLauncher, QuarantineStore, SignatureChecker
              (Authenticode), ScanHistoryStore, UpdateChecker (GitHub Releases), ArchiveChecker
              (zip password detection)
UI/           MainBar (pill chinh), ResultPanel + ResultPanelForm (xo xuong, ho tro ca file lan
              URL-mode), SettingsForm (co them Network/proxy section), DonateForm, HistoryForm,
              ChecksumForm, UrlScanForm, BatchLookupForm
Bootstrapper/ DotNetBootstrapper (kiem tra/cai .NET Framework)
Assets/       tcb.jpg, momo.jpg (nhung vao assembly, dung cho DonateForm)
```

- Project kieu cu (non-SDK-style csproj), `.NET Framework 4.7.2`, dependency qua
  `packages.config` + HintPath (khong dung PackageReference - da thu va bi loi voi
  `dotnet build`/msbuild ngoai VS, xem lich su quyet dinh trong hoi thoai).
- UI toan bo tu code tay (khong dung WinForms Designer), dung Guna.UI2.WinForms cho theme toi.
- Toan bo UI-facing text da chuyen sang **tieng Anh** (comment trong code van la tieng Viet
  khong dau).

## Danh sach tinh nang da lam (theo thu tu buoc)

### Buoc 1-6 (spec goc trong CLAUDE.md)
1. Model + interface + skeleton
2. MainBar UI (pill toi mau, icon shield, dropdown nguon, nut Scan)
3. FileHasher (SHA-256/MD5/SHA-1, single-pass streaming)
4. VirusTotalProvider (API v3 that, upload+poll, da user test OK)
5. MetaDefenderProvider + OpenTipProvider (API that, OpenTIP da user test OK, MetaDefender
   **chua test that**)
6. DotNetBootstrapper (registry check + silent install + repair, asInvoker manifest thay vi
   requireAdministrator toan bo app - chi elevate luc can cai .NET)

### Settings UI (BYOK)
- SettingsForm: nhap/validate/luu API key cho 3 nguon, luu ma hoa DPAPI trong
  `%AppData%\FileScanTool\settings.dat`, tu dong migrate 1 lan tu App.config cu.
- Lazy-prompt: neu bam Scan ma thieu key, tu dong mo Settings dung tab nguon do
  (`MissingApiKeyException`).

### Theme polish
- Doi mau amber choi (`#f2a623`) sang cam dat tram hon (`#d85a30`), icon shield doi sang mau
  trung tinh. Them HoverState cho cac nut/input.

### System tray + Bootstrapper phu
- NotifyIcon voi menu: Cai dat API key, Khoa vi tri, Khoi dong cung he thong (registry Run key
  + co `--silent` de khong bung pill khi Windows tu khoi dong), Lich su quet, Gop y, Ung ho tac
  gia, Kiem tra cap nhat, Thoat. Left-click toggle an/hien, double-click dua ra truoc.
- Keo pill: KHONG dung Guna2DragControl (bi loi vi cac control con chan MouseDown) - tu lam bang
  `WM_NCLBUTTONDOWN`/`HTCAPTION` gan vao cac vung nen (pnlPill, lblShield, pnlHashBar).
  Tuy chon "Khoa vi tri" (mac dinh khoa) quyet dinh co cho keo hay khong.
- Esc de thoat app.

### 18 y tuong mo rong (da lam, tru #3 lan dau bi tu choi roi lam lai sau khi giai thich)
1. Keo-tha file vao pill (drag & drop)
2. Balloon tip khi phat hien virus
3. Nut Copy hash (SHA-256/MD5/SHA-1) vao clipboard
4. Xac nhan truoc khi upload file that len provider (dialog Yes/No)
5. *(Shell context menu "Scan with..." - CHUA LAM, user chua yeu cau lam)*
6. Canh bao gioi han dung luong file ngay khi chon file (truoc khi bam Scan)
7. Kiem tra chu ky Authenticode (SignatureChecker) - chi phat hien chu ky NHUNG TRUC TIEP, khong
   phat hien duoc catalog-signing (vd cac file he thong Windows nhu notepad.exe) - da test that
   voi Node.js/VSCode/Git/Chrome (dung) va notepad.exe (bao "khong co chu ky" - dung nhu gioi han
   da biet, khong phai bug)
8. Lich su quet (ScanHistoryStore, JSON tai `%AppData%\FileScanTool\history.json`, toi da 200
   entry) + HistoryForm de xem/xoa
9. Toan bo UI-facing text da chuyen sang tieng Anh (khong phai chuan hoa dau tieng Viet nhu du
   dinh ban dau - user doi y giua chung)
10. Auto-update check qua GitHub Releases API - **`GitHubRepo` dang de trong** trong
    `Services/UpdateChecker.cs:21`, chua hoat dong cho toi khi user dien repo that vao
11. Khoi dong im lang khi Windows tu chay app (xem System tray o tren)
12. Nut Xoa file / Cach ly file trong tab "Dinh virus" cua ResultPanel (QuarantineStore chuyen
    file vao `%AppData%\FileScanTool\Quarantine\`)
13. Global hotkey `Ctrl+Alt+S` - quet nhanh file dang co trong clipboard (Ctrl+C tu Explorer)
14. Nut "View details" mo trang ket qua cong khai cua nguon (**URL VirusTotal chac chan dung,
    MetaDefender kha tin, OpenTIP KHONG CHAC CHAN dung - chua verify duoc**)
15. Balloon tip chao 1 lan duy nhat tu tray luc dau (HasShownTrayOnboarding flag)

### Sau khi hoi "con y tuong nao khac" (vong 2)
- A. Tra cuu tu lich su local truoc khi goi API that (ScanHistoryStore.FindRecent, cache 30
  ngay) - hien ket qua cu + nut "Rescan" de ep quet lai that. Da verify bang 4 test case.
- B. Verify checksum thu cong (ChecksumForm) - dan hash ky vong, tool tu nhan MD5/SHA-1/SHA-256
  theo do dai va so khop. Truy cap qua chuot phai vao pill -> "Verify checksum..."
- C. Phat hien file .zip co mat khau (ArchiveChecker, doc general purpose bit flag trong local
  file header) - da verify bang file zip that (7-Zip). **Chi ho tro ZIP, khong ho tro RAR/7z.**
- F. Xuat bao cao quet ra file .txt (nut "Export" trong ResultPanel)

### Da tu choi / khong lam
- Quota estimate local (đếm request offline) - tu choi vi khong the chinh xac (drift voi server
  that, 1 lan Scan co the ton nhieu request thuc te do polling)
- D. Phat hien gia mao phan mo rong file (magic bytes vs extension) - user noi "de cho AV lam"
- E. Nut mo thu muc chua file - user noi "de cho AV lam"
- Auto-scan real-time thu muc Downloads - KHONG nen lam, di nguoc tinh than tool (quet theo yeu
  cau) va se dot quota rat nhanh (4 req/phut VirusTotal)

### Vong 3 (sau khi ra soat lai PROGRESS.md, user chon 4 y tuong khong lan sang AV)
- **Scan URL** (`UrlScanForm`, chuot phai vao pill -> "Scan a URL..."): chi ho tro VirusTotal (tra
  cuu truoc bang url_id = base64url(url) khong padding qua `GET urls/{id}`, 404 thi `POST urls` +
  poll `GET analyses/{id}` - cung schema stats/results nhu file) va OpenTIP (`GET
  search/url?request=`, dung `Zone` -> IsClean, khong co danh sach engine rieng). **Da test that
  voi `https://example.com` qua VirusTotal - ra ket qua Clean (92) dung nhu VT gui that.**
  MetaDefender **CHU DONG khong lam** - co doc thay endpoint `GET url/{observable_url}` nhung
  khong lay duoc schema JSON chi tiet qua research, doan sai co the hien nham "sach" cho URL doc
  hai nen khong chap nhan duoc; dropdown chi con VirusTotal/OpenTIP, co ghi chu giai thich ngay
  trong dialog.
- `ResultPanel` them che do URL (`ShowLoadingForUrl`): an nut Delete/Quarantine (khong co file
  local), an "View details" (chua xac minh duoc link dung cho URL), hien "URL: ..." thay vi
  SHA-256/MD5. Tab Infected/Clean, Export bao cao van dung chung code cu.
- **Batch hash lookup** (`BatchLookupForm`, chuot phai vao pill -> "Batch hash lookup..."): dan
  nhieu hash (moi dong 1), chon 1 nguon (ca 3, vi chi goi `LookupByHashAsync` co san, khong upload
  gi), chay tuan tu voi delay giua cac lan goi (16s cho VirusTotal, 3s cho 2 nguon con lai - tranh
  rate-limit). Ket qua tim thay duoc ghi vao `ScanHistoryStore` nhu luong Scan thuong. **Da test
  that qua OpenTIP: hash rong -> "No cached result", hash sai dinh dang -> "Invalid format", MD5
  cua file test EICAR -> "Infected - 48 detections" (du lieu that tu Kaspersky).**
- **Proxy config** (`Services/HttpClientFactory.cs` + section "Network (proxy)" trong
  `SettingsForm`): 3 che do System (mac dinh, khong lam gi them)/Manual (nhap host:port + user/pass
  tuy chon)/None. Ap dung cho ca 3 provider qua `HttpClientFactory.Create()`. **Moi smoke-test che
  do System (mac dinh) khong pha luong quet file hien co va layout UI khong de len nhau - CHUA
  test Manual proxy voi mot proxy server that** (chi test UI enable/disable field dung).
- **Quota display** (best-effort, trong nut "Test" cua SettingsForm): VirusTotal goi `GET
  users/{apikey}` (dung thang API key lam `{id}` du doc chinh thuc ghi la username) - **da test
  that va CO hoat dong**, hien vd "Key is valid - 5/500 requests used today". MetaDefender goi
  `GET apikey/limits/status`, parse phong thu tim field ten pho bien (used/limit/allowed/...) -
  **da test that nhung KHONG nhan dien duoc field nao voi key hien tai**, tu dong roi ve chi hien
  "Key is valid" (dung nhu thiet ke an toan - tha khong hien con hon hien sai). OpenTIP: khong co
  endpoint quota cong khai, luon bo qua.

## Nhung diem CAN NGUOI DUNG XAC NHAN / CHUA CHAC CHAN

1. **MetaDefender upload body format**: dang dung `multipart/form-data` theo doc chinh thuc
   (opswat.com), nhung co 1 nguon cu hon noi dung raw binary + header `filename`. Chua test
   that upload (chi test lookup qua nut "Kiem tra" trong Settings).
2. **MetaDefender file size limit** (~140MB trong `MetaDefenderProvider.cs`): la uoc tinh, khong
   phai so chinh thuc tu OPSWAT.
3. **OpenTIP "View details" URL** (`https://opentip.kaspersky.com/{sha256}/results?tab=lookup`):
   khong tim duoc xac nhan chinh thuc, co the sai dinh dang.
4. **Auto-update chua hoat dong**: can dien `GitHubRepo` trong `Services/UpdateChecker.cs`.
5. **Lich su quet da bi xoa mot lan** (do loi test cua Claude - dung nham file that thay vi ban
   sao cach ly khi test `ScanHistoryStore.Clear()`). Khong anh huong toi API key/settings.
6. **MetaDefender quota** (`MetaDefenderProvider.GetQuotaAsync`, endpoint `apikey/limits/status`):
   da goi that nhung chua nhan dien duoc field nao trong response - co the endpoint/field ten khac
   voi doan, hoac tai khoan hien tai khong tra ve schema do da doc. Neu can quota MetaDefender
   chinh xac, phai xem response JSON that (vd qua Fiddler/browser dev tools) roi sua lai
   `TryExtractQuota`.
7. **MetaDefender URL scan bi vo hieu hoa co chu dich** (khong phai bug) - xem chi tiet trong muc
   "Vong 3" o tren. Neu sau nay tim duoc schema chinh thuc cho `GET url/{observable_url}` thi co
   the bat lai trong `MetaDefenderProvider.ScanUrlAsync` va them "MetaDefender" vao dropdown
   `UrlScanForm`.
8. **Manual proxy chua duoc test voi proxy server that** - chi verify code build/logic va UI
   enable/disable field dung, chua co moi truong proxy that de test luc lam. Neu dung trong moi
   truong co proxy cong ty, nen thu va bao lai neu request bi loi bat thuong.

## Quy uoc lam viec da thiet lap trong hoi thoai

- Lam tung buoc nho, build + smoke-test (`dotnet build` + chay thu exe qua PowerShell
  `Start-Process`/`Stop-Process`) sau MOI thay doi, KHONG dồn nhieu thay doi roi build 1 lan.
- Voi logic quan trong (hash, DPAPI storage, registry, zip parsing...), viet **test doc lap**
  (project .NET tam trong scratchpad) de verify thay vi chi tin code review - roi don dep sau.
  CAN THAN: neu test dung chung file/registry key voi app that (vd `%AppData%\FileScanTool\...`),
  phai co doan don dep dung cach hoac dung duong dan rieng - da co bai hoc xuong mau vu xoa lich
  su o tren.
- Sau App.config sua tay, phai REBUILD moi thay doi duoc phan anh vao `bin\Debug\*.exe.config`
  (da gay nham lan 1 lan).
- User thich duoc hoi/xac nhan truoc khi doi huong lon, nhung cho phep Claude tu quyet cac chi
  tiet ky thuat nho va bao cao lai sau.
- Khong tu them tinh nang AV-detection sau ngoai pham vi user yeu cau (vd tu choi "phat hien gia
  mao extension", "real-time scan folder").
