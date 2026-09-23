using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

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

        public static void AddToVariable(string variable, string value)
        {
            Environment.SetEnvironmentVariable(
                variable,
                CombineVariable(variable, value));
        }

        public static void SetToPahtVariable(string newDirectory)
        {
            Environment.SetEnvironmentVariable(
                "PATH",
                CombineVariable("PATH", newDirectory));
        }

        public static string CombineVariable(string variable, string value)
        {
            if (string.IsNullOrEmpty(variable))
            {
                throw new NullReferenceException($"Variable {nameof(variable)} must not be null or empty");
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new NullReferenceException($"Variable {nameof(value)} must not be null or empty");
            }

            var content = Environment.GetEnvironmentVariable(variable);

            content = string.IsNullOrEmpty(content)
                ? value
                : value + PathEnvironmentSeparator + content;

            return content;
        }

        public static bool IsProgramAvailable(string? programName)
        {

            if (string.IsNullOrEmpty(programName))
            {
                return false;
            }

            var path = Path.GetDirectoryName(programName);
            if (false == string.IsNullOrEmpty(path))
            {
                path = Path.GetFullPath(ReplacePathSeparatorsOnly(path));

                programName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.GetFileNameWithoutExtension(programName)
                    : Path.GetFileName(programName);

                path = CombineVariable("PATH", path);
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

            if (null != path)
            {
                process.StartInfo.Environment["PATH"] = path;
            }

            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return 0 == process.ExitCode && false == string.IsNullOrEmpty(output);
        }

        public static async Task<bool> IsProgramAvailableAsync(string? programName)
        {
            if (string.IsNullOrEmpty(programName))
            {
                return false;
            }

            var path = Path.GetDirectoryName(programName);
            if (false == string.IsNullOrEmpty(path))
            {
                path = Path.GetFullPath(ReplacePathSeparatorsOnly(path));

                programName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.GetFileNameWithoutExtension(programName)
                    : Path.GetFileName(programName);
               
                path = CombineVariable("PATH", path);
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

            if(null != path)
            {
                process.StartInfo.Environment["PATH"] = path;
            }

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
