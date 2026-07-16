using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using frm_scan_online.Models;

namespace frm_scan_online.Services
{
    public static class FileHasher
    {
        private const int BufferSize = 1024 * 1024; // 1MB

        public static Task<FileHashInfo> ComputeAsync(string filePath, CancellationToken ct)
        {
            return Task.Run(() => Compute(filePath, ct), ct);
        }

        private static FileHashInfo Compute(string filePath, CancellationToken ct)
        {
            using (var md5 = MD5.Create())
            using (var sha1 = SHA1.Create())
            using (var sha256 = SHA256.Create())
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan))
            {
                var buffer = new byte[BufferSize];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                }

                md5.TransformFinalBlock(buffer, 0, 0);
                sha1.TransformFinalBlock(buffer, 0, 0);
                sha256.TransformFinalBlock(buffer, 0, 0);

                return new FileHashInfo
                {
                    Md5 = ToHex(md5.Hash),
                    Sha1 = ToHex(sha1.Hash),
                    Sha256 = ToHex(sha256.Hash)
                };
            }
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
