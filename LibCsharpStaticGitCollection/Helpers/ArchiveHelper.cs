using System.Formats.Tar;
using System.IO.Compression;

namespace Chris82111.LibCsharpStaticGitCollection.Helpers
{
    internal class ArchiveHelper
    {
        public static async Task ExtractZipToDirectory(string sourceZipFilePath, string? targetDirectory = null, bool overwriteFiles = true)
        {
            if (string.IsNullOrEmpty(targetDirectory))
            {
                targetDirectory = ".";
            }

            await Task.Run(() =>
                ZipFile.ExtractToDirectory(
                    sourceZipFilePath,
                    targetDirectory,
                    overwriteFiles: true));
        }

        public static async Task ExtractTarGzToDirectory(string sourceTarGzFilePath, string? targetDirectory = null, bool overwriteFiles = true)
        {
            if (string.IsNullOrEmpty(sourceTarGzFilePath))
            {
                throw new ArgumentNullException(nameof(sourceTarGzFilePath));
            }

            if (string.IsNullOrEmpty(targetDirectory))
            {
                targetDirectory = ".";
            }

            var fileName = Path.GetFileName(sourceTarGzFilePath);
            if (false == fileName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid file type, only '.tar.gz' is allowed.");
            }

            string tarFilePath = Path.Combine(targetDirectory, fileName.Substring(0, fileName.Length - 3));

            Directory.CreateDirectory(targetDirectory);

            await DecompressTarGzToTarAsync(sourceTarGzFilePath, tarFilePath, overwriteFiles);

            await TarFile.ExtractToDirectoryAsync(
                tarFilePath,
                targetDirectory,
                overwriteFiles: overwriteFiles);

            File.Delete(tarFilePath);
        }

        private static async Task DecompressTarGzToTarAsync(string sourceTarGzFilePath, string tarFilePath, bool overwriteFiles = true)
        {
            if (false == overwriteFiles && File.Exists(tarFilePath))
            {
                throw new IOException($"Cannot create '{tarFilePath}' because a file or directory with the same name already exists.");
            }

            await using FileStream original = File.OpenRead(sourceTarGzFilePath);
            await using FileStream decompressed = File.Create(tarFilePath);
            await using GZipStream gzip = new GZipStream(original, CompressionMode.Decompress);

            await gzip.CopyToAsync(decompressed);
        }
    }
}
