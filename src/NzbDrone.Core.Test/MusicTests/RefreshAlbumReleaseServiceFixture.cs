using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshAlbumReleaseServiceFixture : CoreTest<RefreshAlbumReleaseService>
    {
        private AlbumRelease _release;
        private List<Track> _tracks;
        private ArtistMetadata _metadata;

        [SetUp]
        public void Setup()
        {
            _release = Builder<AlbumRelease>
                .CreateNew()
                .With(s => s.Media = new List<Medium> { new Medium { Number = 1 } })
                .With(s => s.ForeignReleaseId = "xxx-xxx-xxx-xxx")
                .With(s => s.Monitored = true)
                .With(s => s.TrackCount = 10)
                .Build();

            _metadata = Builder<ArtistMetadata>.CreateNew().Build();

            _tracks = Builder<Track>
                .CreateListOfSize(10)
                .All()
                .With(x => x.AlbumReleaseId = _release.Id)
                .With(x => x.ArtistMetadata = _metadata)
                .With(x => x.ArtistMetadataId = _metadata.Id)
                .BuildList();

            Mocker.GetMock<ITrackService>()
                .Setup(s => s.GetTracksForRefresh(_release.Id, It.IsAny<IEnumerable<string>>()))
                .Returns(_tracks);
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed_and_no_clash()
        {
            var newInfo = _release.JsonClone();
            newInfo.ForeignReleaseId = _release.ForeignReleaseId + 1;
            newInfo.OldForeignReleaseIds = new List<string> { _release.ForeignReleaseId };
            newInfo.Tracks = _tracks;

            Subject.RefreshEntityInfo(_release, new List<AlbumRelease> { newInfo }, false, false, null);

            Mocker.GetMock<IReleaseService>()
                .Verify(v => v.UpdateMany(It.Is<List<AlbumRelease>>(s => s.First().ForeignReleaseId == newInfo.ForeignReleaseId)));
        }

        [Test]
        public void should_merge_if_musicbrainz_id_changed_and_new_already_exists()
        {
            var existing = _release;

            var clash = existing.JsonClone();
            clash.Id = 100;
            clash.ForeignReleaseId = clash.ForeignReleaseId + 1;

            clash.Tracks = Builder<Track>.CreateListOfSize(10)
                .All()
                .With(x => x.AlbumReleaseId = clash.Id)
                .With(x => x.ArtistMetadata = _metadata)
                .With(x => x.ArtistMetadataId = _metadata.Id)
                .BuildList();

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleaseByForeignReleaseId(clash.ForeignReleaseId, false))
                .Returns(clash);

            Mocker.GetMock<ITrackService>()
                .Setup(x => x.GetTracksForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(_tracks);

            var newInfo = existing.JsonClone();
            newInfo.ForeignReleaseId = _release.ForeignReleaseId + 1;
            newInfo.OldForeignReleaseIds = new List<string> { _release.ForeignReleaseId };
            newInfo.Tracks = _tracks;

            Subject.RefreshEntityInfo(new List<AlbumRelease> { clash, existing }, new List<AlbumRelease> { newInfo }, false, false);

            // check old album is deleted
            Mocker.GetMock<IReleaseService>()
                .Verify(v => v.DeleteMany(It.Is<List<AlbumRelease>>(x => x.First().ForeignReleaseId == existing.ForeignReleaseId)));

            // check that clash gets updated
            Mocker.GetMock<IReleaseService>()
                .Verify(v => v.UpdateMany(It.Is<List<AlbumRelease>>(s => s.First().ForeignReleaseId == newInfo.ForeignReleaseId)));
        }

        [Test]
        public void child_merge_targets_should_not_be_null_if_target_is_new()
        {
            var oldTrack = Builder<Track>
                .CreateNew()
                .With(x => x.AlbumReleaseId = _release.Id)
                .With(x => x.ArtistMetadata = _metadata)
                .With(x => x.ArtistMetadataId = _metadata.Id)
                .Build();
            _release.Tracks = new List<Track> { oldTrack };

            var newInfo = _release.JsonClone();
            var newTrack = oldTrack.JsonClone();
            newTrack.ArtistMetadata = _metadata;
            newTrack.ArtistMetadataId = _metadata.Id;
            newTrack.ForeignTrackId = "new id";
            newTrack.OldForeignTrackIds = new List<string> { oldTrack.ForeignTrackId };
            newInfo.Tracks = new List<Track> { newTrack };

            Mocker.GetMock<ITrackService>()
                .Setup(s => s.GetTracksForRefresh(_release.Id, It.IsAny<IEnumerable<string>>()))
                .Returns(new List<Track> { oldTrack });

            Subject.RefreshEntityInfo(_release, new List<AlbumRelease> { newInfo }, false, false, null);

            Mocker.GetMock<IRefreshTrackService>()
                .Verify(v => v.RefreshTrackInfo(It.IsAny<List<Track>>(),
                                                It.IsAny<List<Track>>(),
                                                It.Is<List<Tuple<Track, Track>>>(x => x.All(y => y.Item2 != null)),
                                                It.IsAny<List<Track>>(),
                                                It.IsAny<List<Track>>(),
                                                It.IsAny<List<Track>>(),
                                                It.IsAny<bool>()));

            Mocker.GetMock<IReleaseService>()
                .Verify(v => v.UpdateMany(It.Is<List<AlbumRelease>>(s => s.First().ForeignReleaseId == newInfo.ForeignReleaseId)));
        }
    }
}
