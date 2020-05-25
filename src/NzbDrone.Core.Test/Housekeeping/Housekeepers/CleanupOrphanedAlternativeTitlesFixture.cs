using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedAlternativeTitleFixture : DbTest<CleanupOrphanedAlternativeTitles, AlternativeTitle>
    {
        [Test]
        public void should_delete_orphaned_alternative_title_items()
        {
            var altTitle = Builder<AlternativeTitle>.CreateNew()
                                              .With(h => h.MovieId = default)
                                              .With(h => h.Language = Language.English)
                                              .BuildNew();

            Db.Insert(altTitle);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_alternative_title_items()
        {
            var movie = Builder<Movie>.CreateNew().BuildNew();

            Db.Insert(movie);

            var altTitle = Builder<AlternativeTitle>.CreateNew()
                                              .With(h => h.MovieId = default)
                                              .With(h => h.Language = Language.English)
                                              .With(b => b.MovieId = movie.Id)
                                              .BuildNew();

            Db.Insert(altTitle);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
