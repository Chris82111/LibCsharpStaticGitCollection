using Chris82111.LibCsharpStaticGitCollection.Dtos;
using Chris82111.LibCsharpStaticGitCollection.Lib;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Chris82111.LibCsharpStaticGitCollection
{
    public static class Local
    {
#warning Description
        public static string? GitCommandStaticWindows { get; } = GitCommandStaticWindowsInit();

        // https://github.com/git-for-windows/git/releases/tag/v2.51.2.windows.1
        private static string? GitCommandStaticWindowsInit()
        {
            var fileInfo = new FileInfo(Path.Combine(MinGitLib.MinGitrelativeOutDirectory, "cmd", "git.exe"));
            if (fileInfo.Exists)
            {
                return fileInfo.FullName;
            }
            return null;
        }

#warning Linux static git command is currently not available
        public static string? GitCommandStaticLinux { get; } = GitCommandStaticLinuxInit();

        private static string? GitCommandStaticLinuxInit()
        {
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

        public static bool IsProgramAvailable(string programName)
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

            processdResults.ExitCode = processdResults.ExitCode;

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

            processdResults.ExitCode = processdResults.ExitCode;

            return processdResults;
        }

        public static string GitVersion()
        {
            return CallGit("-v").StandardOutput;
        }

        public static async Task<string> GitVersionAsync()
        {
            return ( await CallGitAsync("-v") ).StandardOutput;
        }
    }
}
