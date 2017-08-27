using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedTrackFilesFixture : DbTest<CleanupOrphanedTrackFiles, TrackFile>
    {
        [Test]
        public void should_delete_orphaned_track_files()
        {
            var trackFile = Builder<TrackFile>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .BuildNew();

            Db.Insert(trackFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_track_files()
        {
            var trackFiles = Builder<TrackFile>.CreateListOfSize(2)
                                                   .All()
                                                   .With(h => h.Quality = new QualityModel())
                                                   .BuildListOfNew();

            Db.InsertMany(trackFiles);

            var track = Builder<Track>.CreateNew()
                                          .With(e => e.TrackFileId = trackFiles.First().Id)
                                          .BuildNew();

            Db.Insert(track);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            Db.All<Track>().Should().Contain(e => e.TrackFileId == AllStoredModels.First().Id);
        }
    }
}
