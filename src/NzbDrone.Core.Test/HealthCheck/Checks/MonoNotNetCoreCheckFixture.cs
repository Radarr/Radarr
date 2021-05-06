using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Processes;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class MonoNotNetCoreCheckFixture : CoreTest<MonoNotNetCoreCheck>
    {
        [SetUp]
        public void setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        [Test]
        [Platform(Exclude = "Mono")]
        public void should_return_ok_if_net_core()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        [Platform("Mono")]
        public void should_log_error_if_mono()
        {
            Subject.Check().ShouldBeError();
        }

        [Test]
        [Platform("Mono")]
        public void should_return_error_if_otherbsd()
        {
            Mocker.GetMock<IProcessProvider>()
                .Setup(x => x.StartAndCapture("uname", null, null))
                .Returns(new ProcessOutput
                    {
                        Lines = new List<ProcessOutputLine>
                        {
                            new ProcessOutputLine(ProcessOutputLevel.Standard, "OpenBSD")
                        }
                    });
            Subject.Check().ShouldBeError();
        }

        [Test]
        [Platform("Mono")]
        public void should_log_error_if_freebsd()
        {
            Mocker.GetMock<IProcessProvider>()
                .Setup(x => x.StartAndCapture("uname", null, null))
                .Returns(new ProcessOutput
                {
                    Lines = new List<ProcessOutputLine>
                    {
                        new ProcessOutputLine(ProcessOutputLevel.Standard, "FreeBSD")
                    }
                });
            Subject.Check().ShouldBeError();
        }
    }
}
