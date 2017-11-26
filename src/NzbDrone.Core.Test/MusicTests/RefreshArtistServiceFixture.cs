using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshArtistServiceFixture : CoreTest<RefreshArtistService>
    {
        private Artist _artist;

        [SetUp]
        public void Setup()
        {
            var season1 = Builder<Album>.CreateNew()
                                         .With(s => s.ForeignAlbumId = "1")
                                         .Build();

            _artist = Builder<Artist>.CreateNew()
                                     .With(s => s.Albums = new List<Album>
                                                            {
                                                                season1
                                                            })
                                     .Build();

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(_artist.Id))
                  .Returns(_artist);
            
            Mocker.GetMock<IProvideArtistInfo>()
                  .Setup(s => s.GetArtistInfo(It.IsAny<string>(), It.IsAny<int>()))
                  .Callback(() => { throw new ArtistNotFoundException(_artist.ForeignArtistId); });
        }

        private void GivenNewArtistInfo(Artist artist)
        {
            Mocker.GetMock<IProvideArtistInfo>()
                  .Setup(s => s.GetArtistInfo(_artist.ForeignArtistId, _artist.MetadataProfileId))
                  .Returns(new Tuple<Artist, List<Album>>(artist, new List<Album>()));
        }

        [Test]
        public void should_monitor_new_albums_automatically()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Albums.Add(Builder<Album>.CreateNew()
                                         .With(s => s.ForeignAlbumId = "2")
                                         .Build());

            GivenNewArtistInfo(newArtistInfo);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.Albums.Count == 2 && s.Albums.Single(season => season.ForeignAlbumId == "2").Monitored == true)));
        }

        [Test]
        public void should_log_error_if_musicbrainz_id_not_found()
        {
            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.IsAny<Artist>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.ForeignArtistId = _artist.ForeignArtistId + 1;

            GivenNewArtistInfo(newArtistInfo);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.ForeignArtistId == newArtistInfo.ForeignArtistId)));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_throw_if_duplicate_album_is_in_existing_info()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Albums.Add(Builder<Album>.CreateNew()
                                         .With(s => s.ForeignAlbumId = "2")
                                         .Build());

            _artist.Albums.Add(Builder<Album>.CreateNew()
                                         .With(s => s.ForeignAlbumId = "2")
                                         .Build());

            _artist.Albums.Add(Builder<Album>.CreateNew()
                                         .With(s => s.ForeignAlbumId = "2")
                                         .Build());

            GivenNewArtistInfo(newArtistInfo);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.Albums.Count == 2)));
        }

        [Test]
        public void should_filter_duplicate_albums()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Albums.Add(Builder<Album>.CreateNew()
                                         .With(s => s.ForeignAlbumId = "2")
                                         .Build());

            newArtistInfo.Albums.Add(Builder<Album>.CreateNew()
                                         .With(s => s.ForeignAlbumId = "2")
                                         .Build());

            GivenNewArtistInfo(newArtistInfo);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.Albums.Count == 2)));

        }
    }
}
