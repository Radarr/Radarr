using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedTracksFixture : DbTest<CleanupOrphanedTracks, Track>
    {
        [Test]
        public void should_delete_orphaned_tracks()
        {
            var track = Builder<Track>.CreateNew()
                                          .BuildNew();

            Db.Insert(track);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_tracks()
        {
            var release = Builder<AlbumRelease>.CreateNew()
                                        .BuildNew();

            Db.Insert(release);

            var tracks = Builder<Track>.CreateListOfSize(2)
                                          .TheFirst(1)
                                          .With(e => e.AlbumReleaseId = release.Id)
                                          .BuildListOfNew();

            Db.InsertMany(tracks);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(e => e.AlbumReleaseId == release.Id);
        }
    }
}
