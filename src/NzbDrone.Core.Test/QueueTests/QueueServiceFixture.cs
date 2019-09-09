using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Music;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Parser.Model;


namespace NzbDrone.Core.Test.QueueTests
{
    [TestFixture]
    public class QueueServiceFixture : CoreTest<QueueService>
    {
        private List<TrackedDownload> _trackedDownloads;

        [SetUp]
        public void SetUp()
        {
            var downloadItem = Builder<NzbDrone.Core.Download.DownloadClientItem>.CreateNew()
                .With(v => v.RemainingTime = TimeSpan.FromSeconds(10))
                .Build();

            var artist = Builder<Artist>.CreateNew()
                .Build();

            var albums = Builder<Album>.CreateListOfSize(3)
                .All()
                .With(e => e.ArtistId = artist.Id)
                .Build();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                .With(r => r.Artist = artist)
                .With(r => r.Albums = new List<Album>(albums))
                .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo())
                .Build();

            _trackedDownloads = Builder<TrackedDownload>.CreateListOfSize(1)
                .All()
                .With(v => v.DownloadItem = downloadItem)
                .With(v => v.RemoteAlbum = remoteAlbum)
                .Build()
                .ToList();

            var historyItem = Builder<History.History>.CreateNew()
                .Build();

            Mocker.GetMock<IHistoryService>()
                .Setup(c => c.Find(It.IsAny<string>(), HistoryEventType.Grabbed)).Returns
                (
                    new List<History.History> { historyItem }
                );
        }

        [Test]
        public void queue_items_should_have_id()
        {
            Subject.Handle(new TrackedDownloadRefreshedEvent(_trackedDownloads));

            var queue = Subject.GetQueue();

            queue.Should().HaveCount(3);

            queue.All(v => v.Id > 0).Should().BeTrue();

            var distinct = queue.Select(v => v.Id).Distinct().ToArray();

            distinct.Should().HaveCount(3);
        }
    }
}
