using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class NetImportStatusCheckFixture : CoreTest<NetImportStatusCheck>
    {
        private List<INetImport> _lists = new List<INetImport>();
        private List<NetImportStatus> _blockedLists = new List<NetImportStatus>();

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<INetImportFactory>()
                  .Setup(v => v.GetAvailableProviders())
                  .Returns(_lists);

            Mocker.GetMock<INetImportStatusService>()
                   .Setup(v => v.GetBlockedProviders())
                   .Returns(_blockedLists);

            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        private Mock<INetImport> GivenList(int i, double backoffHours, double failureHours)
        {
            var id = i;

            var mockList = new Mock<INetImport>();
            mockList.SetupGet(s => s.Definition).Returns(new NetImportDefinition { Id = id });
            mockList.SetupGet(s => s.EnableAuto).Returns(true);

            _lists.Add(mockList.Object);

            if (backoffHours != 0.0)
            {
                _blockedLists.Add(new NetImportStatus
                {
                    ProviderId = id,
                    InitialFailure = DateTime.UtcNow.AddHours(-failureHours),
                    MostRecentFailure = DateTime.UtcNow.AddHours(-0.1),
                    EscalationLevel = 5,
                    DisabledTill = DateTime.UtcNow.AddHours(backoffHours)
                });
            }

            return mockList;
        }

        [Test]
        public void should_not_return_error_when_no_indexers()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_warning_if_indexer_unavailable()
        {
            GivenList(1, 10.0, 24.0);
            GivenList(2, 0.0, 0.0);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_error_if_all_indexers_unavailable()
        {
            GivenList(1, 10.0, 24.0);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_warning_if_few_indexers_unavailable()
        {
            GivenList(1, 10.0, 24.0);
            GivenList(2, 10.0, 24.0);
            GivenList(3, 0.0, 0.0);

            Subject.Check().ShouldBeWarning();
        }
    }
}
