using System;
using WorkflowEngine.Core.Common;

namespace WorkflowEngine.Core.Entities;

public class FileMetadata : BaseEntity
{
    public required string StoredFileName { get; set; } // Guid + Extension
    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }
    public long Size { get; set; }
    public string? UploadedBy { get; set; } // UserId
}
