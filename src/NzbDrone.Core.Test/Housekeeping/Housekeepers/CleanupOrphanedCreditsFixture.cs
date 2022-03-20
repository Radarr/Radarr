using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedCreditsFixture : DbTest<CleanupOrphanedCredits, Credit>
    {
        [Test]
        public void should_delete_orphaned_credit_items()
        {
            var credit = Builder<Credit>.CreateNew()
                                              .With(h => h.MovieMetadataId = default)
                                              .With(h => h.Name = "Some Credit")
                                              .BuildNew();

            Db.Insert(credit);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_credit_items()
        {
            var movieMetadata = Builder<MovieMetadata>.CreateNew().BuildNew();

            Db.Insert(movieMetadata);

            var credit = Builder<Credit>.CreateNew()
                                              .With(h => h.MovieMetadataId = default)
                                              .With(h => h.Name = "Some Credit")
                                              .With(b => b.MovieMetadataId = movieMetadata.Id)
                                              .BuildNew();

            Db.Insert(credit);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
