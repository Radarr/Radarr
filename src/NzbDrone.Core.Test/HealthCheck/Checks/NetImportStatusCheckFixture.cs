using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class ImportListStatusCheckFixture : CoreTest<ImportListStatusCheck>
    {
        private List<IImportList> _lists = new List<IImportList>();
        private List<ImportListStatus> _blockedLists = new List<ImportListStatus>();

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.GetAvailableProviders())
                  .Returns(_lists);

            Mocker.GetMock<IImportListStatusService>()
                   .Setup(v => v.GetBlockedProviders())
                   .Returns(_blockedLists);

            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        private Mock<IImportList> GivenList(int i, double backoffHours, double failureHours)
        {
            var id = i;

            var mockList = new Mock<IImportList>();
            mockList.SetupGet(s => s.Definition).Returns(new ImportListDefinition { Id = id });
            mockList.SetupGet(s => s.EnableAuto).Returns(ImportListType.Automatic);

            _lists.Add(mockList.Object);

            if (backoffHours != 0.0)
            {
                _blockedLists.Add(new ImportListStatus
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
