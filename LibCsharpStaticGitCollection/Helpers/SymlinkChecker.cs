using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Chris82111.LibCsharpStaticGitCollection.Helpers
{
    internal static class SymlinkChecker
    {
        // Import the native symlink function
        [SupportedOSPlatform("linux")]
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern int symlink(string target, string linkpath);

        [SupportedOSPlatform("linux")]
        public static void EnsureSymlinkSupported(string? directory = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new PlatformNotSupportedException(
                    "Symlink support check using libc.symlink() is only valid on Linux. " +
                    "Do not call this method on Windows or macOS.");
            }

            if (string.IsNullOrEmpty(directory))
            {
                directory = ".";
            }

            string linkPath = Path.Combine(directory, "symlink_test_link");

            // First, check write access
            if (!IsWritable(directory))
            {
                throw new InvalidOperationException(
                    $"Cannot write to the directory '{directory}'. Extraction requires write permissions.");
            }

            // Now attempt to create a symlink to a nonexistent target
            int result = symlink("symlink_test_target_nonexistent", linkPath);
            if (result != 0)
            {
                int errno = Marshal.GetLastWin32Error();
                throw new InvalidOperationException(
                    $"Cannot create symlinks in the directory '{directory}'. Filesystem may not support symlinks (errno={errno}).");
            }

            // Cleanup
            File.Delete(linkPath);
        }

        private static bool IsWritable(string directory)
        {
            try
            {
                string testFile = Path.Combine(directory, "write_test.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
