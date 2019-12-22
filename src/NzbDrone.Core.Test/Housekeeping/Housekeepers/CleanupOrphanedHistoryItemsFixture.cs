using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Languages;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedHistoryItemsFixture : DbTest<CleanupOrphanedHistoryItems, History.History>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .BuildNew();
        }

        private void GivenSeries()
        {
            Db.Insert(_movie);
        }

        [Test]
        public void should_delete_orphaned_items()
        {

            var history = Builder<History.History>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.Languages = new List<Language>())
                                                  .BuildNew();
            Db.Insert(history);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned()
        {
            GivenSeries();

            var history = Builder<History.History>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.Languages = new List<Language>())
                                                  .With(h => h.MovieId = _movie.Id)
                                                  .BuildNew();
            Db.Insert(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
