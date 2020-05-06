using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, MovieHistory>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void should_read_write_dictionary()
        {
            var history = Builder<MovieHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel())
                .With(c => c.Languages = new List<Language>())
                .BuildNew();

            history.Data.Add("key1", "value1");
            history.Data.Add("key2", "value2");

            Subject.Insert(history);

            StoredModel.Data.Should().HaveCount(2);
        }

        [Test]
        public void should_get_download_history()
        {
            var historyBluray = Builder<MovieHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.MovieId = 12)
                .With(c => c.EventType = MovieHistoryEventType.Grabbed)
                .BuildNew();

            var historyDvd = Builder<MovieHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.DVD))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.MovieId = 12)
                .With(c => c.EventType = MovieHistoryEventType.Grabbed)
             .BuildNew();

            Subject.Insert(historyBluray);
            Subject.Insert(historyDvd);

            var downloadHistory = Subject.FindDownloadHistory(12, new QualityModel(Quality.Bluray1080p));

            downloadHistory.Should().HaveCount(1);
        }

        [Test]
        public void should_get_movie_history()
        {
            var historyMovie1 = Builder<MovieHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.MovieId = 12)
                .With(c => c.EventType = MovieHistoryEventType.Grabbed)
                .BuildNew();

            var historyMovie2 = Builder<MovieHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.MovieId = 13)
                .With(c => c.EventType = MovieHistoryEventType.Grabbed)
             .BuildNew();

            Subject.Insert(historyMovie1);
            Subject.Insert(historyMovie2);

            var movieHistory = Subject.GetByMovieId(12, null);

            movieHistory.Should().HaveCount(1);
        }

        [Test]
        public void should_sort_movie_history_by_date()
        {
            var historyFirst = Builder<MovieHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.MovieId = 12)
                .With(c => c.EventType = MovieHistoryEventType.MovieFileRenamed)
                .With(c => c.Date = DateTime.UtcNow)
                .BuildNew();

            var historySecond = Builder<MovieHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.MovieId = 12)
                .With(c => c.EventType = MovieHistoryEventType.Grabbed)
                .With(c => c.Date = DateTime.UtcNow.AddMinutes(10))
             .BuildNew();

            Subject.Insert(historyFirst);
            Subject.Insert(historySecond);

            var movieHistory = Subject.GetByMovieId(12, null);

            movieHistory.Should().HaveCount(2);
            movieHistory.First().EventType.Should().Be(MovieHistoryEventType.Grabbed);
        }
    }
}
