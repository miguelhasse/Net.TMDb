using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.TMDb
{
    public interface IImageStorage
    {
        Task DownloadAsync(string fileName, Stream outputStream, CancellationToken cancellationToken);
    }
}
