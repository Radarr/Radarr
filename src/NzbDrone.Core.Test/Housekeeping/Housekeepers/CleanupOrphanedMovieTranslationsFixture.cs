using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMovieTranslationsFixture : DbTest<CleanupOrphanedMovieTranslations, MovieTranslation>
    {
        [Test]
        public void should_delete_orphaned_movie_translation_items()
        {
            var translation = Builder<MovieTranslation>.CreateNew()
                                              .With(h => h.MovieId = default)
                                              .With(h => h.Language = Language.English)
                                              .BuildNew();

            Db.Insert(translation);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_movie_translation_items()
        {
            var movie = Builder<Movie>.CreateNew().BuildNew();

            Db.Insert(movie);

            var translation = Builder<MovieTranslation>.CreateNew()
                                              .With(h => h.MovieId = default)
                                              .With(h => h.Language = Language.English)
                                              .With(b => b.MovieId = movie.Id)
                                              .BuildNew();

            Db.Insert(translation);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
