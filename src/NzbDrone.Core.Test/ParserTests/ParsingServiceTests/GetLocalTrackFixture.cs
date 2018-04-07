using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetLocalTrackFixture : CoreTest<ParsingService>
    {
        private Artist _fakeArtist;
        private Album _fakeAlbum;
        private Track _fakeTrack;
        private ParsedTrackInfo _parsedTrackInfo;

        [SetUp]
        public void Setup()
        {
            _fakeArtist = Builder<Artist>
                .CreateNew()
                .Build();

            _fakeAlbum = Builder<Album>
                .CreateNew()
                .With(e => e.ArtistId = _fakeArtist.Id)
                .With(e => e.Releases = new List<AlbumRelease>
                {
                    new AlbumRelease
                    {
                        Id = "5ecd552b-e54b-4c37-b62c-9d6234834bad"
                    }
                })
                .Build();

            _fakeTrack = Builder<Track>
                .CreateNew()
                .With(e => e.ArtistId = _fakeArtist.Id)
                .With(e => e.AlbumId = _fakeAlbum.Id)
                .With(e => e.Album = null)
                .Build();

            _parsedTrackInfo = Builder<ParsedTrackInfo>
                .CreateNew()
                .With(e => e.AlbumTitle = _fakeAlbum.Title)
                .With(e => e.Title = _fakeTrack.Title)
                .With(e => e.ArtistTitle = _fakeArtist.Name)
                .Build();

            Mocker.GetMock<IAlbumService>()
                .Setup(s => s.FindByTitle(_fakeArtist.Id,_fakeAlbum.Title))
                .Returns(_fakeAlbum);

            Mocker.GetMock<IAlbumService>()
                .Setup(s => s.FindAlbumByRelease(_fakeAlbum.Releases.First().Id))
                .Returns(_fakeAlbum);

            Mocker.GetMock<ITrackService>()
                .Setup(s => s.FindTrackByTitle(_fakeArtist.Id, _fakeAlbum.Id, It.IsAny<int>(), _fakeTrack.Title))
                .Returns(_fakeTrack);
        }

        private void HasAlbumTitleNoReleaseId()
        {
            _parsedTrackInfo.AlbumTitle = _fakeAlbum.Title;
            _parsedTrackInfo.ReleaseMBId = "";
        }

        private void HasReleaseMbIdNoTitle()
        {
            _parsedTrackInfo.AlbumTitle = "";
            _parsedTrackInfo.ReleaseMBId = _fakeAlbum.Releases.First().Id;
        }

        private void HasNoReleaseIdOrTitle()
        {
            _parsedTrackInfo.AlbumTitle = "";
            _parsedTrackInfo.ReleaseMBId = "";
        }

        [Test]
        public void should_find_album_with_title_no_MBID()
        {
            HasAlbumTitleNoReleaseId();

            var localTrack = Subject.GetLocalTrack("somfile.mp3", _fakeArtist, _parsedTrackInfo);

            localTrack.Artist.Id.Should().Be(_fakeArtist.Id);
            localTrack.Album.Id.Should().Be(_fakeAlbum.Id);
            localTrack.Tracks.First().Id.Should().Be(_fakeTrack.Id);
        }

        [Test]
        public void should_find_album_with_release_MBID_no_title()
        {
            HasReleaseMbIdNoTitle();

            var localTrack = Subject.GetLocalTrack("somfile.mp3", _fakeArtist, _parsedTrackInfo);

            localTrack.Artist.Id.Should().Be(_fakeArtist.Id);
            localTrack.Album.Id.Should().Be(_fakeAlbum.Id);
            localTrack.Tracks.First().Id.Should().Be(_fakeTrack.Id);
        }

        [Test]
        public void should_not_find_album_with_no_release_MBID_no_title()
        {
            HasNoReleaseIdOrTitle();
            
            var localTrack = Subject.GetLocalTrack("somfile.mp3", _fakeArtist, _parsedTrackInfo);
            ExceptionVerification.ExpectedWarns(1);

            localTrack.Should().BeNull();

        }
    }
}
