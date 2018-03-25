using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Qualities;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, History.History>
    {
        [SetUp]
        public void Setup()
        {
            QualityDefinitionServiceFixture.SetupDefaultDefinitions();
        }

        [Test]
        public void should_read_write_dictionary()
        {
            var history = Builder<History.History>.CreateNew()
                .With(c => c.Quality = new QualityModel())
                .BuildNew();

            history.Data.Add("key1", "value1");
            history.Data.Add("key2", "value2");

            Subject.Insert(history);

            StoredModel.Data.Should().HaveCount(2);
        }


        [Test]
        public void should_get_download_history()
        {
            var historyBluray = Builder<History.History>.CreateNew()
                .With(c => c.Quality = new QualityModel(QualityWrapper.Dynamic.Bluray1080p))
                .With(c => c.MovieId = 12)
                .With(c => c.EventType = HistoryEventType.Grabbed)
                .BuildNew();

            var historyDvd = Builder<History.History>.CreateNew()
                .With(c => c.Quality = new QualityModel(QualityWrapper.Dynamic.DVD))
                .With(c => c.MovieId = 12)
                .With(c => c.EventType = HistoryEventType.Grabbed)
             .BuildNew();

            Subject.Insert(historyBluray);
            Subject.Insert(historyDvd);

            var downloadHistory = Subject.FindDownloadHistory(12, new QualityModel(QualityWrapper.Dynamic.Bluray1080p));

            downloadHistory.Should().HaveCount(1);
        }

    }
}
