using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMovieMetadataFixture : DbTest<CleanupOrphanedMovieMetadata, MovieMetadata>
    {
        [Test]
        public void should_delete_orphaned_movie_metadata_items()
        {
            var metadata = Builder<MovieMetadata>.CreateNew().BuildNew();

            Db.Insert(metadata);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_movie_metadata_items()
        {
            var movieMetadata = Builder<MovieMetadata>.CreateNew().BuildNew();

            Db.Insert(movieMetadata);

            var movie = Builder<Movie>.CreateNew()
                                              .With(b => b.MovieMetadataId = movieMetadata.Id)
                                              .BuildNew();

            Db.Insert(movie);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_not_delete_unorphaned_movie_metadata_items_for_lists()
        {
            var movieMetadata = Builder<MovieMetadata>.CreateNew().BuildNew();

            Db.Insert(movieMetadata);

            var movie = Builder<ImportListMovie>.CreateNew()
                                              .With(b => b.MovieMetadataId = movieMetadata.Id)
                                              .BuildNew();

            Db.Insert(movie);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
