using System.IO;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public interface IImageStorage
    {
        Task DownloadAsync(string fileName, Stream outputStream, CancellationToken cancellationToken);
    }
}

#pragma warning restore 1591