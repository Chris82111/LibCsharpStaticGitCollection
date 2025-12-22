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

        public static async Task ExtractArchives(string? archivePath, string? destination)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var files = new List<string>(4);

                var fileName = Path.GetFileName(GitWindowsLib.GitWindowsOutputZipFileRelative);

                if (false == string.IsNullOrEmpty(archivePath))
                {
                    files.Add(EnvironmentHelper.ExpandEnvironmentVariables(Path.Combine(archivePath, fileName)));
                }
                
                files.Add(EnvironmentHelper.ExpandEnvironmentVariables(Path.Combine(BaseDirectory, fileName)));

                files.Add(EnvironmentHelper.ExpandEnvironmentVariables(Path.Combine(Directory.GetCurrentDirectory(), fileName)));

                files.Add(EnvironmentHelper.ExpandEnvironmentVariables(Path.Combine(BaseDirectory, GitWindowsLib.GitWindowsOutputZipFileRelative)));

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

                if (EnvironmentHelper.IsDirectoryMissingOrEmpty(output))
                {
                    await ArchiveHelper.ExtractZipToDirectory(
                        archive,
                        output,
                        overwriteFiles: true);
                }

                GitCommandStaticWindows = EnvironmentHelper.ReplacePathSeparatorsOnly(Path.Combine(output, GitWindowsLib.GitWindowsOutputExecutableRelative));
                GitCommand = GitCommandStaticWindows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var files = new List<string>(4);

                var fileName = Path.GetFileName(GitLinuxLib.GitLinuxOutputZipFileRelative);

                if (false == string.IsNullOrEmpty(archivePath))
                {
                    files.Add(EnvironmentHelper.ExpandEnvironmentVariables(Path.Combine(archivePath, fileName)));
                }

                files.Add(EnvironmentHelper.ExpandEnvironmentVariables(Path.Combine(BaseDirectory, fileName)));

                files.Add(EnvironmentHelper.ExpandEnvironmentVariables(Path.Combine(Directory.GetCurrentDirectory(), fileName)));

                files.Add(EnvironmentHelper.ExpandEnvironmentVariables(Path.Combine(BaseDirectory, GitLinuxLib.GitLinuxOutputZipFileRelative)));

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

                if (EnvironmentHelper.IsDirectoryMissingOrEmpty(output))
                {
                    await ArchiveHelper.ExtractTarGzToDirectory(
                        archive,
                        output,
                        overwriteFiles: true);
                }

                GitCommandStaticLinux = EnvironmentHelper.ReplacePathSeparatorsOnly(Path.Combine(output, GitLinuxLib.GitLinuxOutputExecutableRelative));
                GitCommand = GitCommandStaticLinux;

                EnvironmentHelper.SetToPahtVariable(Path.Combine(output, "bin"));
                Environment.SetEnvironmentVariable("GIT_PREFIX", output);
                Environment.SetEnvironmentVariable("GIT_EXEC_PATH", Path.Combine(output, "libexec", "git-core"));
                Environment.SetEnvironmentVariable("GIT_TEMPLATE_DIR", Path.Combine(output, "share", "git-core", "templates"));
                Environment.SetEnvironmentVariable("GIT_SSL_CAINFO", Path.Combine(output, "ca", "ca.pem"));

                EnvironmentHelper.SetToVariable("LD_LIBRARY_PATH", Path.Combine(output, "openssl", "lib64"));
                EnvironmentHelper.SetToVariable("LD_LIBRARY_PATH", Path.Combine(output, "curl", "lib"));
            }

            return;
        }

        public static bool IsGitAvailable()
        {
            return EnvironmentHelper.IsProgramAvailable(GitCommand);
        }

        public static async Task<bool> IsGitAvailableAsync()
        {
            return await EnvironmentHelper.IsProgramAvailableAsync(GitCommand);
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
