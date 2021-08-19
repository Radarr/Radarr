using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBlocklistFixture : DbTest<CleanupOrphanedBlocklist, Blocklist>
    {
        [Test]
        public void should_delete_orphaned_blocklist_items()
        {
            var blocklist = Builder<Blocklist>.CreateNew()
                                              .With(h => h.MovieId = default)
                                              .With(h => h.Quality = new QualityModel())
                                              .With(h => h.Languages = new List<Language>())
                                              .BuildNew();

            Db.Insert(blocklist);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_blocklist_items()
        {
            var movie = Builder<Movie>.CreateNew().BuildNew();

            Db.Insert(movie);

            var blocklist = Builder<Blocklist>.CreateNew()
                                              .With(h => h.MovieId = default)
                                              .With(h => h.Quality = new QualityModel())
                                              .With(h => h.Languages = new List<Language>())
                                              .With(b => b.MovieId = movie.Id)
                                              .BuildNew();

            Db.Insert(blocklist);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
