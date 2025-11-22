using Chris82111.LibCsharpStaticGitCollection.Lib;
using System.Runtime.InteropServices;

namespace Chris82111.LibCsharpStaticGitCollection.TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (OperatingSystem.IsWindows())
            {
                EnableVTSupport();
            }

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

            try
            {
                Console.WriteLine($"{color}Windows{reset}");
                Local.GitCommand = Local.GitCommandStaticWindows;
                Console.WriteLine($"{color}Version  {reset}: {Local.GitVersion()}");
                Console.WriteLine($"{color}Available{reset}: {Local.IsGitAvailable()}");
            }
            catch { ; }

            try
            {
                Console.WriteLine($"{color}Linux{reset}");
                Local.GitCommand = Local.GitCommandStaticLinux;
                Console.WriteLine($"{color}Version  {reset}: {Local.GitVersion()}");
                Console.WriteLine($"{color}Available{reset}: {Local.IsGitAvailable()}");
            }
            catch { ; }
        }

        static void ReworkPath(ref string path)
        {
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
