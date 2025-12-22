using System.Runtime.InteropServices;

namespace Chris82111.LibCsharpStaticGitCollection.TestConsoleAppNuGet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (OperatingSystem.IsWindows())
            {
                EnableVTSupport();
            }

            Local.ExtractArchives().Wait();

            var color = "\x1b[1;49;33m";
            var reset = "\x1b[0m";


            Console.WriteLine($"{color}GitCommand             {reset}: {Local.GitCommand}");
            Console.WriteLine($"{color}GitCommandStaticLinux  {reset}: {Local.GitCommandStaticLinux}");
            Console.WriteLine($"{color}GitCommandStaticWindows{reset}: {Local.GitCommandStaticWindows}");

            Console.WriteLine($"{color}Available (git)                    {reset}: {Local.IsProgramAvailable("git")}");
            Console.WriteLine($"{color}Available (GitCommand)             {reset}: {Local.IsProgramAvailable(Local.GitCommand)}");
            Console.WriteLine($"{color}Available (GitCommandStaticLinux)  {reset}: {Local.IsProgramAvailable(Local.GitCommandStaticLinux)}");
            Console.WriteLine($"{color}Available (GitCommandStaticWindows){reset}: {Local.IsProgramAvailable(Local.GitCommandStaticWindows)}");
            Console.WriteLine($"{color}Available                          {reset}: {Local.IsGitAvailable()}");

            Console.WriteLine($"{color}Version{reset}: {Local.GitVersion()}");

            var gitCommand = Local.GitCommand;

            try
            {
                Console.WriteLine($"{color}Windows{reset}");
                Local.GitCommand = Local.GitCommandStaticWindows ?? string.Empty;
                Console.WriteLine($"{color}Version  {reset}: {Local.GitVersion()}");
                Console.WriteLine($"{color}Available{reset}: {Local.IsGitAvailable()}");
            }
            catch {; }

            try
            {
                Console.WriteLine($"{color}Linux{reset}");
                Local.GitCommand = Local.GitCommandStaticLinux ?? string.Empty;
                Console.WriteLine($"{color}Version  {reset}: {Local.GitVersion()}");
                Console.WriteLine($"{color}Available{reset}: {Local.IsGitAvailable()}");
            }
            catch {; }

            Local.GitCommand = gitCommand;

            try
            {
                var result = Local.CallGit(@"clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git");
                Console.WriteLine($"{color}ExitCode      {reset}: {result.ExitCode}");
                Console.WriteLine($"{color}StandardOutput{reset}: {result.StandardOutput}");
                Console.WriteLine($"{color}StandardError {reset}: {result.StandardError}");
            }
            catch {; }
        }

        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private static void EnableVTSupport()
        {
            IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (handle == IntPtr.Zero)
                return;

            if (!GetConsoleMode(handle, out uint mode))
                return;

            // Add the flag that enables ANSI escape sequences
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            SetConsoleMode(handle, mode);
        }
    }
}
