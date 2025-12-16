using System.IO;
using System.Threading.Tasks;

namespace WorkflowEngine.Core.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName);
    Task<(Stream FileStream, string ContentType, string OriginalFileName)> DownloadAsync(string fileName);
}
