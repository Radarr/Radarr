using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using System.Collections.Generic;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBlacklistFixture : DbTest<CleanupOrphanedBlacklist, Blacklist>
    {
        [Test]
        public void should_delete_orphaned_blacklist_items()
        {
            var blacklist = Builder<Blacklist>.CreateNew()
                                              .With(h => h.MovieId = new int())
                                              .With(h => h.Quality = new QualityModel())
                                              .With(h => h.Languages = new List<Language>())
                                              .BuildNew();

            Db.Insert(blacklist);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_blacklist_items()
        {
            var movie = Builder<Movie>.CreateNew().BuildNew();

            Db.Insert(movie);

            var blacklist = Builder<Blacklist>.CreateNew()
                                              .With(h => h.MovieId = new int())
                                              .With(h => h.Quality = new QualityModel())
                                              .With(h => h.Languages = new List<Language>())
                                              .With(b => b.MovieId = movie.Id)
                                              .BuildNew();

            Db.Insert(blacklist);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
