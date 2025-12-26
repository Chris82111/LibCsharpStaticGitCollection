namespace Chris82111.LibCsharpStaticGitCollection.TestConsoleApp
{
    internal class Program
    {
        static void Main()
        {
            Local.ExtractArchives().Wait();

            var color = "\x1b[1;49;33m";
            var reset = "\x1b[0m";

            Console.WriteLine($"{color}GitCommand             {reset}: {Local.GitCommand}");
            Console.WriteLine($"{color}GitCommandStaticLinux  {reset}: {Local.GitCommandStaticLinux}");
            Console.WriteLine($"{color}GitCommandStaticWindows{reset}: {Local.GitCommandStaticWindows}");
            Console.WriteLine($"{color}Available              {reset}: {Local.IsGitAvailable()}");

            Console.WriteLine($"{color}Version{reset}: {Local.GitVersion()}");

            var gitCommand = Local.GitCommand;

            try
            {
                Console.WriteLine($"{color}Windows{reset}");
                Local.GitCommand = Local.GitCommandStaticWindows ?? string.Empty;
                Console.WriteLine($"{color}Version  {reset}: {Local.GitVersion()}");
                Console.WriteLine($"{color}Available{reset}: {Local.IsGitAvailable()}");
            }
            catch { ; }

            try
            {
                Console.WriteLine($"{color}Linux{reset}");
                Local.GitCommand = Local.GitCommandStaticLinux ?? string.Empty;
                Console.WriteLine($"{color}Version  {reset}: {Local.GitVersion()}");
                Console.WriteLine($"{color}Available{reset}: {Local.IsGitAvailable()}");
            }
            catch { ; }

            Local.GitCommand = gitCommand;

            try
            {
                var result = Local.CallGit(@"clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git");
                Console.WriteLine($"{color}ExitCode      {reset}: {result.ExitCode}");
                Console.WriteLine($"{color}StandardOutput{reset}: {result.StandardOutput}");
                Console.WriteLine($"{color}StandardError {reset}: {result.StandardError}");
            }
            catch { ; }
        }
    }
}
