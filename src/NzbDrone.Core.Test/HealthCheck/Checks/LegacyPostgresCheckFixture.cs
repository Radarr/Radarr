using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class LegacyPostgresCheckFixture : CoreTest<LegacyPostgresCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Warning {0} -> {1}");
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var name in new[] { "__Postgres__Host", "__Postgres__Port", ":Postgres:Host", ":Postgres:Port" })
            {
                Environment.SetEnvironmentVariable(BuildInfo.AppName + name, null);
            }
        }

        [Test]
        public void should_return_ok_normally()
        {
            Subject.Check().ShouldBeOk();
        }

        [TestCase("__")]
        [TestCase(":")]
        public void should_return_error_if_vars_defined(string separator)
        {
            Environment.SetEnvironmentVariable(BuildInfo.AppName + separator + "Postgres" + separator + "Host", "localhost");
            Environment.SetEnvironmentVariable(BuildInfo.AppName + separator + "Postgres" + separator + "Port", "localhost");

            var result = Subject.Check();
            result.ShouldBeError("Warning " + BuildInfo.AppName + separator + "Postgres" + separator + "Host, " +
                                 BuildInfo.AppName + separator + "Postgres" + separator + "Port -> " +
                                 BuildInfo.AppName + separator + "PostgresHost, " +
                                 BuildInfo.AppName + separator + "PostgresPort");
        }
    }
}
