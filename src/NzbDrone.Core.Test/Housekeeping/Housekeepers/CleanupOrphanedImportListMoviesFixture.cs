using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedImportListMoviesFixture : DbTest<CleanupOrphanedImportListMovies, ImportListMovie>
    {
        private ImportListDefinition _importList;

        [SetUp]
        public void Setup()
        {
            _importList = Builder<ImportListDefinition>.CreateNew()
                                                       .BuildNew();
        }

        private void GivenImportList()
        {
            Db.Insert(_importList);
        }

        [Test]
        public void should_delete_orphaned_importlistmovies()
        {
            var status = Builder<ImportListMovie>.CreateNew()
                                                 .With(h => h.ListId = _importList.Id)
                                                 .BuildNew();
            Db.Insert(status);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_importlistmovies()
        {
            GivenImportList();

            var movieMetadata = Builder<MovieMetadata>.CreateNew().BuildNew();

            Db.Insert(movieMetadata);

            var status = Builder<ImportListMovie>.CreateNew()
                                                 .With(h => h.ListId = _importList.Id)
                                                 .With(b => b.MovieMetadataId = movieMetadata.Id)
                                                 .BuildNew();
            Db.Insert(status);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.ListId == _importList.Id);
        }
    }
}
