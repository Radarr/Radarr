using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class SqliteVersionCheckFixture : CoreTest<SqliteVersionCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        private void GivenOutput(string version)
        {
            if (!OsInfo.IsLinux)
            {
                throw new IgnoreException("linux specific test");
            }

            Mocker.GetMock<IMainDatabase>()
                  .SetupGet(s => s.Version)
                  .Returns(new Version(version));
        }

        [TestCase("3.9.0")]
        [TestCase("3.9.1")]
        [TestCase("3.26.0")]
        [TestCase("3.34.0")]
        public void should_return_ok(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeOk();
        }

        [TestCase("3.8.11.1")]
        [TestCase("3.8.11")]
        [TestCase("3.3.9")]
        [TestCase("2.8.12")]
        public void should_return_error(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeError();
        }
    }
}
