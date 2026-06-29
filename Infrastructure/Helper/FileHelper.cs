using Application.Helpers;
using Domain.Constants;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.Helper
{
    public class FileHelper : IFileHelper
    {
        private const int DefaultImageQuality = 80;

        private readonly string _storagePath;
        private readonly string _tempStoragePath;
        private readonly List<FileOperation> _pendingOperations = new();

        private class FileOperation
        {
            public string TempPath { get; set; } = string.Empty;
            public string FinalPath { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
        }

        public FileHelper()
        {
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _storagePath = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
            _tempStoragePath = Path.GetFullPath(Path.Combine(webRoot, "uploads", "temp"));

            EnsureDirectoryExists(_storagePath);
            EnsureDirectoryExists(_tempStoragePath);
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string filePrefix = "")
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            
            if (!AppConstants.AllowedFileExtensions.Contains(ext)) 
                throw new UnauthorizedAccessException($"Extension {ext} is not allowed.");

            bool isImage = AppConstants.ImageExtensions.Contains(ext);
            var secureName = GenerateSecureFileName(fileName, filePrefix);

            if (isImage) secureName = Path.ChangeExtension(secureName, ".webp");

            var tempPath = Path.GetFullPath(Path.Combine(_tempStoragePath, secureName));
            var finalPath = Path.GetFullPath(Path.Combine(_storagePath, secureName));

            ValidatePathIsWithinDirectory(tempPath, _tempStoragePath);
            ValidatePathIsWithinDirectory(finalPath, _storagePath);

            fileStream.Position = 0;

            if (isImage)
            {
                using var inputStream = new SKManagedStream(fileStream);
                using var original = SKBitmap.Decode(inputStream);
                if (original == null) throw new InvalidDataException("Uploaded file is not a valid image format.");

                using var image = SKImage.FromBitmap(original);
                using var data = image.Encode(SKEncodedImageFormat.Webp, DefaultImageQuality);
                using var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
                data.SaveTo(fs);
            }
            else
            {
                await using var outputStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
                await fileStream.CopyToAsync(outputStream);
            }

            _pendingOperations.Add(new FileOperation { TempPath = tempPath, FinalPath = finalPath, FileName = secureName });
            return secureName;
        }

        public async Task CommitFilesAsync(List<string> filenames)
        {
            if (filenames == null || !filenames.Any()) return;
            var opsToCommit = _pendingOperations.Where(op => filenames.Contains(op.FileName)).ToList();

            foreach (var op in opsToCommit)
            {
                if (File.Exists(op.TempPath))
                {
                    await Task.Run(() =>
                    {
                        if (File.Exists(op.FinalPath)) File.Delete(op.FinalPath);
                        File.Move(op.TempPath, op.FinalPath);
                    });
                }
            }
        }

        public async Task RollbackFilesAsync(List<string> filenames)
        {
            if (filenames == null || !filenames.Any()) return;
            var opsToRollback = _pendingOperations.Where(op => filenames.Contains(op.FileName)).ToList();

            foreach (var op in opsToRollback)
            {
                if (File.Exists(op.TempPath)) await Task.Run(() => File.Delete(op.TempPath));
                if (File.Exists(op.FinalPath)) await Task.Run(() => File.Delete(op.FinalPath));
            }
            _pendingOperations.Clear();
        }

        public IEnumerable<string> GetUploadedFileNames() => _pendingOperations.Select(op => op.FileName).ToList();

        public async Task DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var safePath = Path.GetFullPath(Path.Combine(_storagePath, Path.GetFileName(fileName)));
            ValidatePathIsWithinDirectory(safePath, _storagePath);

            if (File.Exists(safePath)) await Task.Run(() => File.Delete(safePath));
        }

        private static string GenerateSecureFileName(string originalName, string prefix)
        {
            var cleanPrefix = Regex.Replace(prefix ?? "", "[^a-zA-Z0-9_-]", "");
            var extension = Path.GetExtension(originalName).ToLowerInvariant();
            return $"{(string.IsNullOrEmpty(cleanPrefix) ? "" : cleanPrefix + "_")}{Guid.NewGuid():N}{extension}";
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private static void ValidatePathIsWithinDirectory(string targetPath, string baseDir)
        {
            string safeBaseDir = Path.GetFullPath(baseDir);
            if (!safeBaseDir.EndsWith(Path.DirectorySeparatorChar.ToString())) safeBaseDir += Path.DirectorySeparatorChar;
            string safeTargetPath = Path.GetFullPath(targetPath);
            if (!safeTargetPath.StartsWith(safeBaseDir, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Attempted file operation outside safe directory limits.");
        }
    }
}