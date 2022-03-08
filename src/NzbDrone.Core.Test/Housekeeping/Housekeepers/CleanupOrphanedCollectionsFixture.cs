using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedCollectionsFixture : DbTest<CleanupOrphanedCollections, MovieCollection>
    {
        [Test]
        public void should_delete_orphaned_collection_item()
        {
            var collection = Builder<MovieCollection>.CreateNew()
                                              .With(h => h.Id = 3)
                                              .With(h => h.TmdbId = 123456)
                                              .With(h => h.Title = "Some Credit")
                                              .BuildNew();

            Db.Insert(collection);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_collection_items()
        {
            var collection = Builder<MovieCollection>.CreateNew()
                                              .With(h => h.Id = 3)
                                              .With(h => h.TmdbId = 123456)
                                              .With(h => h.Title = "Some Credit")
                                              .BuildNew();

            Db.Insert(collection);

            var movie = Builder<MovieMetadata>.CreateNew().With(m => m.CollectionTmdbId = collection.TmdbId).BuildNew();

            Db.Insert(movie);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
