using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed class StorageClient : IImageStorage
    {
        public async Task DownloadAsync(string fileName, Stream outputStream, CancellationToken cancellationToken)
        {
            var requestUri = new Uri(String.Concat(@"http://image.tmdb.org/t/p/original", fileName));
            HttpClientHandler handler = new HttpClientHandler
            {
                PreAuthenticate = true,
                UseDefaultCredentials = true
            };
            var response = await (new HttpClient(handler)).GetAsync(requestUri,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var content = response.EnsureSuccessStatusCode().Content;
            await content.CopyToAsync(outputStream).ConfigureAwait(false);
        }
    }
}

#pragma warning restore 1591