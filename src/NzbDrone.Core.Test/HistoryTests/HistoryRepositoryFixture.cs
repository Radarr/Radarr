using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, MovieHistory>
    {
        private Movie _movie1;
        private Movie _movie2;

        [SetUp]
        public void Setup()
        {
            _movie1 = Builder<Movie>.CreateNew()
                                    .With(s => s.Id = 7)
                                    .Build();

            _movie2 = Builder<Movie>.CreateNew()
                                    .With(s => s.Id = 8)
                                    .Build();
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

        [Test]
        public void should_delete_history_items_by_movieId()
        {
            var items = Builder<MovieHistory>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.MovieId = _movie2.Id)
                .TheRest()
                .With(c => c.MovieId = _movie1.Id)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.EventType = MovieHistoryEventType.Grabbed)
                .BuildListOfNew();

            Db.InsertMany(items);

            Subject.DeleteForMovies(new List<int> { _movie1.Id });

            var removedItems = Subject.GetByMovieId(_movie1.Id, null);
            var nonRemovedItems = Subject.GetByMovieId(_movie2.Id, null);

            removedItems.Should().HaveCount(0);
            nonRemovedItems.Should().HaveCount(1);
        }
    }
}
