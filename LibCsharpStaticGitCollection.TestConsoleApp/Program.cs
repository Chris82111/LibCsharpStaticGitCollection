using Chris82111.LibCsharpStaticGitCollection.Lib;

namespace Chris82111.LibCsharpStaticGitCollection.TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string zipPath = @"./Lib/MinGit-2.51.2-64-bit.zip";
            string extractPath = @"./Lib/MinGit-2.51.2-64-bit";
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

            var color = "\x1b[1;49;33m";
            var reset = "\x1b[0m";
            Console.WriteLine($"{color}Static Linux   Git path {reset}: {Local.GitCommandStaticLinux}");
            Console.WriteLine($"{color}Static Windows Git path {reset}: {Local.GitCommandStaticWindows}");
            Console.WriteLine($"{color}Static Git path         {reset}: {Local.GitCommand}");

            Console.WriteLine($"{color}Git                {reset}: {Local.IsProgramAvailable("git")}");
            Console.WriteLine($"{color}Static Git         {reset}: {Local.IsProgramAvailable(Local.GitCommand)}");
            Console.WriteLine($"{color}Static Git Linux   {reset}: {Local.IsProgramAvailable(Local.GitCommandStaticLinux)}");
            Console.WriteLine($"{color}Static Git Windows {reset}: {Local.IsProgramAvailable(Local.GitCommandStaticWindows)}");

            // Local.GitCommand default
            Console.WriteLine($"{color}Version   {reset}: {Local.GitVersion()}");
            Console.WriteLine($"{color}Available {reset}: {Local.IsGitAvailable()}");
            Console.WriteLine($"{color}Version   {reset}: {Local.CallGit("-v").StandardOutput}");

            try
            {
                Local.GitCommand = Local.GitCommandStaticWindows;
                Console.WriteLine($"{color}Version   {reset}: {Local.GitVersion()}");
                Console.WriteLine($"{color}Available {reset}: {Local.IsGitAvailable()}");
                Console.WriteLine($"{color}Version   {reset}: {Local.CallGit("-v").StandardOutput}");
            }
            catch { ; }

            try
            {
                Local.GitCommand = Local.GitCommandStaticLinux;
                Console.WriteLine($"{color}Version   {reset}: {Local.GitVersion()}");
                Console.WriteLine($"{color}Available {reset}: {Local.IsGitAvailable()}");
                Console.WriteLine($"{color}Version   {reset}: {Local.CallGit("-v").StandardOutput}");
            }
            catch { ; }
        }
    }
}
