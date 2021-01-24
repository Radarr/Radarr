using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedPendingReleasesFixture : DbTest<CleanupOrphanedPendingReleases, PendingRelease>
    {
        [Test]
        public void should_delete_orphaned_pending_items()
        {
            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.ParsedBookInfo = new ParsedBookInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_pending_items()
        {
            var author = Builder<Author>.CreateNew().BuildNew();

            Db.Insert(author);

            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.AuthorId = author.Id)
                .With(h => h.ParsedBookInfo = new ParsedBookInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
