using Chris82111.LibCsharpStaticGitCollection.Dtos;
using Chris82111.LibCsharpStaticGitCollection.Lib;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Chris82111.LibCsharpStaticGitCollection
{
    public static class Local
    {
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
                GitLinuxLib.GitLinuxRelativeOutputZipDirectory,
                overwriteFiles: overwriteFiles);

            File.Delete(tarFilePath);
        }

        public static async Task ExtractArchives()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (false == Directory.Exists(GitWindowsLib.GitWindowsRelativeOutputZipDirectory))
                {
                    await Task.Run(() => 
                    ZipFile.ExtractToDirectory(
                        GitWindowsLib.GitWindowsRelativeOutputZipFile, 
                        GitWindowsLib.GitWindowsRelativeOutputZipDirectory,
                        overwriteFiles: true));
		        }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (false == Directory.Exists(GitLinuxLib.GitLinuxRelativeOutputZipDirectory))
                {
                    await ExtractTarGzToDirectory(
                        GitLinuxLib.GitLinuxRelativeOutputZipFile,
                        GitLinuxLib.GitLinuxRelativeOutputZipDirectory,
                        overwriteFiles: true);
                }

                SetToPahtVariable(GitLinuxLib.GitLinuxRelativeOutputZipDirectory);
                Environment.SetEnvironmentVariable("GIT_PREFIX", GitLinuxLib.GitLinuxRelativeOutputZipDirectory);
                Environment.SetEnvironmentVariable("GIT_EXEC_PATH", Path.Combine(GitLinuxLib.GitLinuxRelativeOutputZipDirectory, "libexec/git-core"));
                Environment.SetEnvironmentVariable("GIT_TEMPLATE_DIR", Path.Combine(GitLinuxLib.GitLinuxRelativeOutputZipDirectory, "share/git-core/templates"));
                Environment.SetEnvironmentVariable("GIT_SSL_CAINFO", Path.Combine(GitLinuxLib.GitLinuxRelativeOutputZipDirectory, "ca/ca.pem"));
            }

            return;
        }

        private static string PathEnvironmentSeparator = OperatingSystem.IsWindows() ? ";" : ":";

        private static void SetToPahtVariable(string newDirectory)
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";

            Environment.SetEnvironmentVariable("PATH", newDirectory + PathEnvironmentSeparator + currentPath);
        }

#warning Description
        public static string? GitCommandStaticWindows { get; } = GitCommandStaticWindowsInit();

        private static string? GitCommandStaticWindowsInit()
        {
            var fileInfo = new FileInfo(GitWindowsLib.GitWindowsRelativeOutputExecutable);
            if (fileInfo.Exists)
            {
                return fileInfo.FullName;
            }
            return null;
        }

        public static string? GitCommandStaticLinux { get; } = GitCommandStaticLinuxInit();

        private static string? GitCommandStaticLinuxInit()
        {
            var fileInfo = new FileInfo(GitLinuxLib.GitLinuxRelativeOutputExecutable);
            if (fileInfo.Exists)
            {
                return fileInfo.FullName;
            }
            return null;
        }

        public static string GitCommand { get; set; } = GitCommandInit() ?? "git";

        private static string? GitCommandInit()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GitCommandStaticWindows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GitCommandStaticLinux;
            }

            return null;
        }

        private static readonly string WhitchCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which";

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
