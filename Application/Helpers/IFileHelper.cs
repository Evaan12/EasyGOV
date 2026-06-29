using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public interface IFileHelper
    {
        // Unifies images and documents. Automatically detects and WebP compresses images.
        Task<string> SaveFileAsync(Stream fileStream, string fileName, string filePrefix = "");
        Task DeleteFileAsync(string fileName);
        Task CommitFilesAsync(List<string> filenames);
        Task RollbackFilesAsync(List<string> filenames);
        IEnumerable<string> GetUploadedFileNames();
    }
}