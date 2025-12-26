using System.Diagnostics;

namespace Chris82111.LibCsharpStaticGitCollection.Helpers
{
    internal class EnvironmentHelper
    {
        /// <summary>
        /// The individual environment variables are separated by different delimiters in Linux (':') and Windows (';') systems.
        /// </summary>
        private static readonly string PathEnvironmentSeparator = OperatingSystem.IsWindows() ? ";" : ":";

        private static readonly string WhitchCommand = OperatingSystem.IsWindows() ? "where" : "which";

        public static bool IsDirectoryMissingOrEmpty(string path)
        {
            bool existsAndNotEmpty = Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any();
            return false == existsAndNotEmpty;
        }

        public static void SetToVariable(string variable, string value)
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

        public static void SetToPahtVariable(string newDirectory)
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
    }
}
