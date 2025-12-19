using System.IO;
using System.Threading.Tasks;

namespace WorkflowEngine.Core.Interfaces;

using System;

public interface IStorageService
{
    Task<Guid> UploadAsync(Stream fileStream, string fileName);
    Task<(Stream FileStream, string ContentType, string OriginalFileName)> DownloadAsync(string fileName);
}
