using System;
using System.Security.Cryptography.X509Certificates;

namespace frm_scan_online.Services
{
    public class SignatureInfo
    {
        public bool IsSigned { get; set; }
        public string Publisher { get; set; }
    }

    // Quick, local-only check for an Authenticode signature (does not verify the chain/trust/revocation
    // over the network - just a fast reference signal, not a substitute for an actual AV scan result).
    public static class SignatureChecker
    {
        public static SignatureInfo Check(string filePath)
        {
            try
            {
                var cert = X509Certificate.CreateFromSignedFile(filePath);
                return new SignatureInfo { IsSigned = true, Publisher = ParseCommonName(cert.Subject) };
            }
            catch
            {
                return new SignatureInfo { IsSigned = false, Publisher = null };
            }
        }

        private static string ParseCommonName(string subject)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return null;
            }

            foreach (var part in subject.Split(','))
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(3).Trim('"');
                }
            }

            return subject;
        }
    }
}
