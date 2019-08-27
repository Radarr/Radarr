using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshTrackServiceFixture : CoreTest<RefreshTrackService>
    {
        private AlbumRelease _release;
        private List<Track> _allTracks;

        [SetUp]
        public void Setup()
        {
            _release = Builder<AlbumRelease>.CreateNew().Build();
            _allTracks = Builder<Track>.CreateListOfSize(20)
                .All()
                .BuildList();
        }

        [Test]
        public void updated_track_should_not_have_null_album_release()
        {
            var add = new List<Track>();
            var update = new List<Track>();
            var merge = new List<Tuple<Track, Track>>();
            var delete = new List<Track>();
            var upToDate = new List<Track>();

            upToDate.AddRange(_allTracks.Take(10));

            var toUpdate = _allTracks[10].JsonClone();
            toUpdate.Title = "title to update";
            toUpdate.AlbumRelease = _release;

            update.Add(toUpdate);

            Subject.RefreshTrackInfo(add, update, merge, delete, upToDate, _allTracks, false);

            Mocker.GetMock<IAudioTagService>()
                .Verify(v => v.SyncTags(It.Is<List<Track>>(x => x.Count == 1 &&
                                                           x[0].AlbumRelease != null &&
                                                           x[0].AlbumRelease.IsLoaded == true)));

        }
    }
}
