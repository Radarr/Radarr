using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMovieVersionsFixture : DbTest<CleanupOrphanedMovieVersions, MovieVersion>
    {
        [Test]
        public void should_delete_orphaned_movie_versions()
        {
            var version = Builder<MovieVersion>.CreateNew()
                                               .With(v => v.MovieId = 1)
                                               .BuildNew();

            Db.Insert(version);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_versions_of_existing_movies()
        {
            var movie = Builder<Movie>.CreateNew()
                                      .BuildNew();

            Db.Insert(movie);

            var version = Builder<MovieVersion>.CreateNew()
                                               .With(v => v.MovieId = movie.Id)
                                               .BuildNew();

            Db.Insert(version);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
