using System.Diagnostics;

namespace LibCsharpStaticGitCollection.TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Version: {GitVersion().Result}");
        }

        public static async Task<string> GitVersion()
        {
            var psi = new ProcessStartInfo
            {
                //FileName = "git",
                FileName = "./Lib/MinGit-2.51.2-64-bit/cmd/git.exe",
                Arguments = "-v",
                WorkingDirectory = ".",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.Environment["GIT_ASKPASS"] = "echo";

            var process = new Process { StartInfo = psi };

            if (null == process)
            {
                return string.Empty;
            }

            process.Start();

            //TODO: Debug
            string output = await process.StandardOutput.ReadToEndAsync();

            await process.WaitForExitAsync();

            return output;            
        }
    }
}
