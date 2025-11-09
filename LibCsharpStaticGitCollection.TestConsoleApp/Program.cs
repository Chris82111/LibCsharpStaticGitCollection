using Chris82111.LibCsharpStaticGitCollection.Lib;

namespace Chris82111.LibCsharpStaticGitCollection.TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Settings : {MinGitLib.MinGitrelativeOutDirectory}");

            Console.WriteLine($"Static Linux   Git path : {Local.GitCommandStaticLinux}");
            Console.WriteLine($"Static Windows Git path : {Local.GitCommandStaticWindows}");
            Console.WriteLine($"Static Git path         : {Local.GitCommand}");

            Console.WriteLine($"Git       : {Local.IsProgramAvailable("git")}");
            Console.WriteLine($"Static Git: {Local.IsProgramAvailable(Local.GitCommand)}");
            Console.WriteLine($"Available : {Local.IsGitAvailable()}");
            Console.WriteLine($"Version   : {Local.CallGit("-v").StandardOutput}");
            Console.WriteLine($"Version   : {Local.GitVersion()}");
        }
    }
}
