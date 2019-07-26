using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Indexers;
using System.Linq;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadServiceFixture : CoreTest<TrackedDownloadService>
    {
        private void GivenDownloadHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<History.History>(){
                 new History.History(){
                     DownloadId = "35238",
                     SourceTitle = "Audio Artist - Audio Album [2018 - FLAC]",
                     ArtistId = 5,
                     AlbumId = 4,
                 }
                });
        }

        [Test]
        public void should_track_downloads_using_the_source_title_if_it_cannot_be_found_using_the_download_title()
        {
            GivenDownloadHistory();

            var remoteAlbum = new RemoteAlbum
            {
                Artist = new Artist() { Id = 5 },
                Albums = new List<Album> { new Album { Id = 4 } },
                ParsedAlbumInfo = new ParsedAlbumInfo()
                {
                    AlbumTitle = "Audio Album",
                    ArtistName = "Audio Artist"
                }
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedAlbumInfo>(i => i.AlbumTitle == "Audio Album" && i.ArtistName == "Audio Artist"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                  .Returns(remoteAlbum);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "The torrent release folder",
                DownloadId = "35238",
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteAlbum.Should().NotBeNull();
            trackedDownload.RemoteAlbum.Artist.Should().NotBeNull();
            trackedDownload.RemoteAlbum.Artist.Id.Should().Be(5);
            trackedDownload.RemoteAlbum.Albums.First().Id.Should().Be(4);
        }

        [Test]
        public void should_unmap_tracked_download_if_album_deleted()
        {
            GivenDownloadHistory();

            var remoteAlbum = new RemoteAlbum
            {
                Artist = new Artist() { Id = 5 },
                Albums = new List<Album> { new Album { Id = 4 } },
                ParsedAlbumInfo = new ParsedAlbumInfo()
                {
                    AlbumTitle = "Audio Album",
                    ArtistName = "Audio Artist"
                }
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedAlbumInfo>(i => i.AlbumTitle == "Audio Album" && i.ArtistName == "Audio Artist"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                  .Returns(remoteAlbum);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "Audio Artist - Audio Album [2018 - FLAC]",
                DownloadId = "35238",
            };

            // get a tracked download in place
            var trackedDownload = Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            // simulate deletion - album no longer maps
            Mocker.GetMock<IParsingService>()
                .Setup(s => s.Map(It.Is<ParsedAlbumInfo>(i => i.AlbumTitle == "Audio Album" && i.ArtistName == "Audio Artist"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .Returns(default(RemoteAlbum));

            // handle deletion event
            Subject.Handle(new AlbumDeletedEvent(remoteAlbum.Albums.First(), false));

            // verify download has null remote album
            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteAlbum.Should().BeNull();
        }
    }
}
