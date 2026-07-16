using System;

namespace frm_scan_online.Providers
{
    public class MissingApiKeyException : InvalidOperationException
    {
        public string ProviderName { get; }

        public MissingApiKeyException(string providerName)
            : base($"No API key configured for {providerName}.")
        {
            ProviderName = providerName;
        }
    }
}
