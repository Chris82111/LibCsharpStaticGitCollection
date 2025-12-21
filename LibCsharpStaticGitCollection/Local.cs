using Chris82111.LibCsharpStaticGitCollection.Dtos;
using Chris82111.LibCsharpStaticGitCollection.Helpers;
using Chris82111.LibCsharpStaticGitCollection.Lib;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Chris82111.LibCsharpStaticGitCollection
{
    public static class Local
    {
        public static string BaseDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;

        public static string GitCommand { get; set; } = "git";

        public static string? GitCommandStaticWindows { get; private set; } = null;

        public static string? GitCommandStaticLinux { get; private set; } = null;

        /// <summary>
        /// The individual environment variables are separated by different delimiters in Linux (':') and Windows (';') systems.
        /// </summary>
        private static readonly string PathEnvironmentSeparator = OperatingSystem.IsWindows() ? ";" : ":";

        private static readonly string WhitchCommand = OperatingSystem.IsWindows() ? "where" : "which";

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

        private static async Task ExtractTarGzToDirectory(string sourceTarGzFilePath, string? targetDirectory = null, bool overwriteFiles = true)
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

        public static string ExpandEnvironmentVariables(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            name = Environment.ExpandEnvironmentVariables(name);

            if (name == "~" || name.StartsWith("~/") || name.StartsWith("~\\"))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                name = Path.Combine(home, name.Length > 2 ? name.Substring(2) : "");
            }

            return Path.GetFullPath(name);
        }

        public static async Task ExtractArchives()
        {
            await ExtractArchives(null, null);
        }

        public static async Task ExtractArchivesFrom(string archivePath)
        {
            await ExtractArchives(archivePath, null);
        }

        public static async Task ExtractArchivesTo(string destination)
        {
            await ExtractArchives(null, destination);
        }

        public static string ReplacePathSeparatorsOnly(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            switch (Path.DirectorySeparatorChar)
            {
                case '/':
                    path = path.Replace('\\', '/');
                    break;
                case '\\':
                    path = path.Replace('/', '\\');
                    break;
                default:
                    path = path
                        .Replace('\\', Path.DirectorySeparatorChar)
                        .Replace('/', Path.DirectorySeparatorChar);
                    break;
            }

            return path;
        }

        public static async Task ExtractArchives(string? archivePath, string? destination)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var files = new List<string>(4);

                var fileName = Path.GetFileName(GitWindowsLib.GitWindowsOutputZipFileRelative);

                if (false == string.IsNullOrEmpty(archivePath))
                {
                    files.Add(ExpandEnvironmentVariables(Path.Combine(archivePath, fileName)));
                }
                
                files.Add(ExpandEnvironmentVariables(Path.Combine(BaseDirectory, fileName)));

                files.Add(ExpandEnvironmentVariables(Path.Combine(Directory.GetCurrentDirectory(), fileName)));

                files.Add(ExpandEnvironmentVariables(Path.Combine(BaseDirectory, GitWindowsLib.GitWindowsOutputZipFileRelative)));

                string? archive = null;

                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        archive = file;
                        break;
                    }
                }

                if(null == archive)
                {
                    throw new FileNotFoundException($"File was no found: {GitWindowsLib.GitWindowsOutputZipFileRelative}");
                }

                if(".zip" != Path.GetExtension(archive))
                {
                    throw new Exception($"File Extension must be .zip");
                }

                var output = Path.Combine(
                    destination ?? Path.Combine(BaseDirectory, ".bin", Path.GetDirectoryName(GitWindowsLib.GitWindowsOutputZipFileRelative) ?? string.Empty),
                    Path.GetFileNameWithoutExtension(archive));

                if (false == Directory.Exists(output))
                {
                    await Task.Run(() =>
                    ZipFile.ExtractToDirectory(
                        archive,
                        output,
                        overwriteFiles: true));
                }

                GitCommandStaticWindows = ReplacePathSeparatorsOnly(Path.Combine(output, GitWindowsLib.GitWindowsOutputExecutableRelative));
                GitCommand = GitCommandStaticWindows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var files = new List<string>(4);

                var fileName = Path.GetFileName(GitLinuxLib.GitLinuxOutputZipFileRelative);

                if (false == string.IsNullOrEmpty(archivePath))
                {
                    files.Add(ExpandEnvironmentVariables(Path.Combine(archivePath, fileName)));
                }

                files.Add(ExpandEnvironmentVariables(Path.Combine(BaseDirectory, fileName)));

                files.Add(ExpandEnvironmentVariables(Path.Combine(Directory.GetCurrentDirectory(), fileName)));

                files.Add(ExpandEnvironmentVariables(Path.Combine(BaseDirectory, GitLinuxLib.GitLinuxOutputZipFileRelative)));

                string? archive = null;

                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        archive = file;
                        break;
                    }
                }

                if (null == archive)
                {
                    throw new FileNotFoundException($"File was no found: {GitLinuxLib.GitLinuxOutputZipFileRelative}");
                }

                var extensionBasedOnLengthsExpected = archive.Substring(archive.Length - 7);
                if (".tar.gz" != extensionBasedOnLengthsExpected)
                {
                    throw new Exception($"File Extension must be .tar.gz");
                }

                fileName = Path.GetFileName(archive);

                var fileNameWithoutExtension = fileName.Substring(0, fileName.Length - 7);

                var output = Path.Combine(
                    destination ?? Path.Combine(BaseDirectory, ".bin", Path.GetDirectoryName(GitLinuxLib.GitLinuxOutputZipFileRelative) ?? string.Empty),
                    fileNameWithoutExtension);

                SymlinkChecker.EnsureSymlinkSupported(output);

                if (false == Directory.Exists(output))
                {
                    await ExtractTarGzToDirectory(
                        archive,
                        output,
                        overwriteFiles: true);
                }

                GitCommandStaticLinux = ReplacePathSeparatorsOnly(Path.Combine(output, GitLinuxLib.GitLinuxOutputExecutableRelative));
                GitCommand = GitCommandStaticLinux;

                SetToPahtVariable(Path.Combine(output, "bin"));
                Environment.SetEnvironmentVariable("GIT_PREFIX", output);
                Environment.SetEnvironmentVariable("GIT_EXEC_PATH", Path.Combine(output, "libexec", "git-core"));
                // error: Warning: templates not found in /media/chris82111/abc/Users/chris82111/source/repos/LibCsharpStaticGitCollection/LibCsharpStaticGitCollection.TestConsoleApp/bin/Debug/net8.0/temp/runtimes/linux-x64/native/GitLinux/share/git-core/templates
                Environment.SetEnvironmentVariable("GIT_TEMPLATE_DIR", Path.Combine(output, "share", "git-core", "templates"));
                Environment.SetEnvironmentVariable("GIT_SSL_CAINFO", Path.Combine(output, "ca", "ca.pem"));

                SetToVariable("LD_LIBRARY_PATH", Path.Combine(output, "openssl", "lib64"));
                SetToVariable("LD_LIBRARY_PATH", Path.Combine(output, "curl", "lib"));
            }

            return;
        }
        private static void SetToVariable(string variable, string value)
        {
            if (string.IsNullOrEmpty(variable))
            {
                throw new NullReferenceException($"Variable {nameof(variable)} must not be null or empty");
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new NullReferenceException($"Variable {nameof(value)} must not be null or empty");
            }

            string? content = Environment.GetEnvironmentVariable(variable);

            content = string.IsNullOrEmpty(content)
                ? value
                : value + PathEnvironmentSeparator + content;

            Environment.SetEnvironmentVariable(variable, content);
        }

        private static void SetToPahtVariable(string newDirectory)
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";

            Environment.SetEnvironmentVariable("PATH", newDirectory + PathEnvironmentSeparator + currentPath);
        }

        public static bool IsProgramAvailable(string? programName)
        {
            if (string.IsNullOrEmpty(programName))
            {
                return false;
            }

            if (File.Exists(programName))
            {
                return true;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = WhitchCommand,
                    Arguments = programName,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return 0 == process.ExitCode && false == string.IsNullOrEmpty(output);
        }

        public static async Task<bool> IsProgramAvailableAsync(string programName)
        {
            if (File.Exists(programName))
            {
                return true;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = WhitchCommand,
                    Arguments = programName,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();

            await process.WaitForExitAsync();

            return 0 == process.ExitCode && false == string.IsNullOrWhiteSpace(output);
        }

        public static bool IsGitAvailable()
        {
            return IsProgramAvailable(GitCommand);
        }

        public static async Task<bool> IsGitAvailableAsync()
        {
            return await IsProgramAvailableAsync(GitCommand);
        }

        public static CallGitProcessdResultsDto CallGit(string gitArguments, string? workingDirectory = null)
        {
            var processdResults = new CallGitProcessdResultsDto();

            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = ".";
            }

            var psi = new ProcessStartInfo
            {
                FileName = GitCommand,
                Arguments = gitArguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.Environment["GIT_ASKPASS"] = "echo";

            var process = new Process { StartInfo = psi };

            if (null == process)
            {
                return processdResults;
            }

            process.Start();

            processdResults.StandardOutput = process.StandardOutput.ReadToEnd();
            processdResults.StandardError = process.StandardError.ReadToEnd();

            process.WaitForExit();

            processdResults.ExitCode = process.ExitCode;

            return processdResults;
        }

        public static async Task<CallGitProcessdResultsDto> CallGitAsync(string gitArguments, string? workingDirectory = null)
        {
            var processdResults = new CallGitProcessdResultsDto();

            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = ".";
            }

            var psi = new ProcessStartInfo
            {
                FileName = GitCommand,
                Arguments = gitArguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.Environment["GIT_ASKPASS"] = "echo";

            var process = new Process { StartInfo = psi };

            if (null == process)
            {
                return processdResults;
            }

            process.Start();

            processdResults.StandardOutput = await process.StandardOutput.ReadToEndAsync();
            processdResults.StandardError = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            processdResults.ExitCode = process.ExitCode;

            return processdResults;
        }

        public static string GitVersion()
        {
            return CallGit("-v").StandardOutput.Replace("\r", null).Replace("\n", null);
        }

        public static async Task<string> GitVersionAsync()
        {
            return ( await CallGitAsync("-v") ).StandardOutput;
        }
    }
}
