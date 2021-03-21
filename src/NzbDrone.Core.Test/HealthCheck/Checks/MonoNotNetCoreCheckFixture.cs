using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Common.Processes;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class MonoNotNetCoreCheckFixture : CoreTest<MonoNotNetCoreCheck>
    {
        [Test]
        [Platform(Exclude = "Mono")]
        public void should_return_ok_if_net_core()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        [Platform("Mono")]
        public void should_log_warning_if_mono()
        {
            Subject.Check().ShouldBeWarning();
        }

        [Test]
        [Platform("Mono")]
        public void should_return_ok_if_otherbsd()
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
            Subject.Check().ShouldBeOk();
        }

        [Test]
        [Platform("Mono")]
        public void should_log_warning_if_freebsd()
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
            Subject.Check().ShouldBeWarning();
        }
    }
}
