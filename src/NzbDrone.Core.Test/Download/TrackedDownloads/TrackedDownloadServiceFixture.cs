using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadServiceFixture : CoreTest<TrackedDownloadService>
    {
        private void GivenDownloadHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<History.History>()
                {
                 new History.History()
                {
                     DownloadId = "35238",
                     SourceTitle = "Audio Artist - Audio Album [2018 - FLAC]",
                     AuthorId = 5,
                     BookId = 4,
                }
                });
        }

        [Test]
        public void should_track_downloads_using_the_source_title_if_it_cannot_be_found_using_the_download_title()
        {
            GivenDownloadHistory();

            var remoteAlbum = new RemoteBook
            {
                Author = new Author() { Id = 5 },
                Books = new List<Book> { new Book { Id = 4 } },
                ParsedBookInfo = new ParsedBookInfo()
                {
                    BookTitle = "Audio Album",
                    AuthorName = "Audio Artist"
                }
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedBookInfo>(i => i.BookTitle == "Audio Album" && i.AuthorName == "Audio Artist"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
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
            trackedDownload.RemoteBook.Should().NotBeNull();
            trackedDownload.RemoteBook.Author.Should().NotBeNull();
            trackedDownload.RemoteBook.Author.Id.Should().Be(5);
            trackedDownload.RemoteBook.Books.First().Id.Should().Be(4);
        }

        [Test]
        public void should_unmap_tracked_download_if_album_deleted()
        {
            GivenDownloadHistory();

            var remoteAlbum = new RemoteBook
            {
                Author = new Author() { Id = 5 },
                Books = new List<Book> { new Book { Id = 4 } },
                ParsedBookInfo = new ParsedBookInfo()
                {
                    BookTitle = "Audio Album",
                    AuthorName = "Audio Artist"
                }
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedBookInfo>(i => i.BookTitle == "Audio Album" && i.AuthorName == "Audio Artist"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
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
                .Setup(s => s.Map(It.Is<ParsedBookInfo>(i => i.BookTitle == "Audio Album" && i.AuthorName == "Audio Artist"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .Returns(default(RemoteBook));

            // handle deletion event
            Subject.Handle(new BookDeletedEvent(remoteAlbum.Books.First(), false, false));

            // verify download has null remote album
            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteBook.Should().BeNull();
        }
    }
}
