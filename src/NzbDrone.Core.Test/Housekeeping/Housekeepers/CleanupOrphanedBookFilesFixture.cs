using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBookFilesFixture : DbTest<CleanupOrphanedBookFiles, BookFile>
    {
        [Test]
        public void should_unlink_orphaned_track_files()
        {
            var trackFile = Builder<BookFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.EditionId = 1)
                .BuildNew();

            Db.Insert(trackFile);
            Subject.Clean();
            AllStoredModels[0].EditionId.Should().Be(0);
        }
    }
}
