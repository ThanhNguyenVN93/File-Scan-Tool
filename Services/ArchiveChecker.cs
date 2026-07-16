using System.IO;
using System.Linq;

namespace frm_scan_online.Services
{
    // Quick, local-only check for whether a .zip file is password-protected, by reading the general
    // purpose bit flag directly from the first local file header (not using System.IO.Compression,
    // since that API doesn't expose the encryption flag). ZIP format only - RAR/7z would need their
    // own parser, not implemented here.
    public static class ArchiveChecker
    {
        private static readonly byte[] LocalFileHeaderSignature = { 0x50, 0x4B, 0x03, 0x04 };

        // Returns true/false when it can be determined, null if it's not a zip file or can't be read.
        public static bool? IsPasswordProtectedZip(string filePath)
        {
            try
            {
                using (var stream = File.OpenRead(filePath))
                using (var reader = new BinaryReader(stream))
                {
                    if (stream.Length < 8)
                    {
                        return null;
                    }

                    var signature = reader.ReadBytes(4);
                    if (!signature.SequenceEqual(LocalFileHeaderSignature))
                    {
                        return null;
                    }

                    reader.ReadInt16(); // version needed to extract
                    var generalPurposeFlag = reader.ReadInt16();
                    return (generalPurposeFlag & 0x1) != 0;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
