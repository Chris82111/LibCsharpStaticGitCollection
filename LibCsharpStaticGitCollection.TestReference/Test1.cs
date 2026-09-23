using Chris82111.LibCsharpStaticGitCollection;
using System.Runtime.InteropServices;

namespace Chris82111.LibCsharpStaticGitCollection.TestReference
{
    [TestClass]
    [DoNotParallelize]
    public sealed class Test1
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public void Test_01_WithoutExtract()
        {
            Local.Reset();

            TestContext.WriteLine((null == Local.GitCommandStaticWindows) ? "null" : Local.GitCommandStaticWindows);
            TestContext.WriteLine((null == Local.GitCommandStaticLinux) ? "null" : Local.GitCommandStaticLinux);
            TestContext.WriteLine((null == Local.GitCommand) ? "null" : Local.GitCommand);

            Assert.IsNull(Local.GitCommandStaticWindows);
            Assert.IsNull(Local.GitCommandStaticLinux);
            Assert.AreEqual("git", Local.GitCommand);
        }

        [TestMethod]
        public void Test_02_Extract()
        {
            Local.Reset();

            Assert.AreEqual("git", Local.GitCommand);

            Local.ExtractArchive();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.AreEqual(Local.GitCommandStaticWindows, Local.GitCommand);
                Assert.IsNull(Local.GitCommandStaticLinux);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Assert.AreEqual(Local.GitCommandStaticLinux, Local.GitCommand);
                Assert.IsNull(Local.GitCommandStaticWindows);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Test_03_IsGitAvailable()
        {
            Local.Reset();

            Local.ExtractArchive();

            Assert.IsTrue(Local.IsGitAvailable());
        }

        [TestMethod]
        public void Test_04_GitVersion()
        {
            Local.Reset();

            Local.ExtractArchive();

            var version1 = Local.GitVersion();

            var call = Local.CallGit("-v");

            var version2 = call.StandardOutput.Replace("\r", null).Replace("\n", null);
            Assert.AreEqual("", call.StandardError);
            Assert.AreEqual(0, call.ExitCode);

            Assert.IsNotNull(version1);
            Assert.IsNotNull(version2);

            Assert.AreEqual(version1, version2);
        }

        [TestMethod]
        public void Test_05_Clone()
        {
            Local.Reset();

            Local.ExtractArchive();

            var directory = new DirectoryInfo("LibCsharpStaticGitCollection");
            Assert.IsFalse(directory.Exists);

            var result = Local.CallGit(@"clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git");
            Assert.AreEqual(0, result.ExitCode);
            Assert.AreEqual("", result.StandardOutput);
            Assert.AreEqual("Cloning into 'LibCsharpStaticGitCollection'...", result.StandardError.Replace("\r", null).Replace("\n", null));

            var onefile = new FileInfo(Path.Combine(directory.FullName, "README.md"));
            Assert.IsTrue(onefile.Exists);

            foreach (var file in Directory.EnumerateFiles(
                directory.FullName,
                "*",
                SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(directory.FullName, recursive: true);
        }
    }
}
