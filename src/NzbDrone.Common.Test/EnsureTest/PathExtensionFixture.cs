using NUnit.Framework;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EnsureTest
{
    [TestFixture]
    public class PathExtensionFixture : TestBase
    {
        [TestCase(@"p:\Music\file with, comma.mp3")]
        [TestCase(@"\\serer\share\file with, comma.mp3")]
        public void EnsureWindowsPath(string path)
        {
            WindowsOnly();
            Ensure.That(path, () => path).IsValidPath();
        }


        [TestCase(@"/var/user/file with, comma.mp3")]
        public void EnsureLinuxPath(string path)
        {
            MonoOnly();
            Ensure.That(path, () => path).IsValidPath();
        }
    }
}
