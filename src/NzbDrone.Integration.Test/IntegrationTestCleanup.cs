using System.IO;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [SetUpFixture]
    public class IntegrationTestSetup
    {
        [OneTimeSetUp]
        [OneTimeTearDown]
        public void CleanUp()
        {
            var dir = Path.Combine(TestContext.CurrentContext.TestDirectory, "CachedAppData");
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
