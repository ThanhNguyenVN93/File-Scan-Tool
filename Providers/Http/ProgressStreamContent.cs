using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace frm_scan_online.Providers.Http
{
    // HttpContent that reads in chunks and reports real upload progress (HttpClient has no built-in
    // mechanism for this)
    internal sealed class ProgressStreamContent : HttpContent
    {
        private const int ChunkSize = 81920;
        private readonly Stream _content;
        private readonly Action<long> _onProgress;

        public ProgressStreamContent(Stream content, Action<long> onProgress)
        {
            _content = content;
            _onProgress = onProgress;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[ChunkSize];
            long totalRead = 0;
            int read;
            while ((read = await _content.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                await stream.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                totalRead += read;
                _onProgress?.Invoke(totalRead);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Length;
            return true;
        }
    }
}
