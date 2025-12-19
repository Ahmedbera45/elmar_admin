using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;

namespace WorkflowEngine.Infrastructure.Services;

public class LocalDiskStorageService : IStorageService
{
    private readonly string _rootPath;
    private readonly AppDbContext _context;

    public LocalDiskStorageService(AppDbContext context)
    {
        _context = context;
        _rootPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
    }

    public async Task<Guid> UploadAsync(Stream fileStream, string fileName)
    {
        var now = DateTime.UtcNow;
        var folderPath = Path.Combine(_rootPath, "Uploads", now.Year.ToString(), now.Month.ToString("00"));

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folderPath, storedFileName);

        using (var fs = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        var relativePath = Path.Combine(now.Year.ToString(), now.Month.ToString("00"), storedFileName);

        var metadata = new FileMetadata
        {
            Id = Guid.NewGuid(),
            StoredFileName = relativePath,
            OriginalFileName = fileName,
            ContentType = "application/octet-stream",
            Size = fileStream.Length,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        _context.FileMetadatas.Add(metadata);
        await _context.SaveChangesAsync();

        return metadata.Id;
    }

    public async Task<(Stream FileStream, string ContentType, string OriginalFileName)> DownloadAsync(string filePath)
    {
        // Sanitize path to prevent traversal
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, "Uploads", filePath));
        if (!fullPath.StartsWith(Path.Combine(_rootPath, "Uploads")))
        {
            throw new UnauthorizedAccessException("Invalid file path.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        // Retrieve Metadata
        var metadata = await _context.FileMetadatas
            .FirstOrDefaultAsync(f => f.StoredFileName == filePath);

        var originalFileName = metadata?.OriginalFileName ?? Path.GetFileName(fullPath);
        var contentType = metadata?.ContentType ?? "application/octet-stream";

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

        return (stream, contentType, originalFileName);
    }
}
