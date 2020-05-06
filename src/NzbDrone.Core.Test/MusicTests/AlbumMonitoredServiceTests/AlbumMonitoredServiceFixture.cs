using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.AlbumMonitoredServiceTests
{
    [TestFixture]
    public class SetAlbumMontitoredFixture : CoreTest<AlbumMonitoredService>
    {
        private Author _artist;
        private List<Book> _albums;

        [SetUp]
        public void Setup()
        {
            const int albums = 4;

            _artist = Builder<Author>.CreateNew()
                                     .Build();

            _albums = Builder<Book>.CreateListOfSize(albums)
                                        .All()
                                        .With(e => e.Monitored = true)
                                        .With(e => e.ReleaseDate = DateTime.UtcNow.AddDays(-7))

                                        //Future
                                        .TheFirst(1)
                                        .With(e => e.ReleaseDate = DateTime.UtcNow.AddDays(7))

                                        //Future/TBA
                                        .TheNext(1)
                                        .With(e => e.ReleaseDate = null)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IAlbumService>()
                  .Setup(s => s.GetAlbumsByArtist(It.IsAny<int>()))
                  .Returns(_albums);

            Mocker.GetMock<IAlbumService>()
                .Setup(s => s.GetArtistAlbumsWithFiles(It.IsAny<Author>()))
                .Returns(new List<Book>());
        }

        [Test]
        public void should_be_able_to_monitor_artist_without_changing_albums()
        {
            Subject.SetAlbumMonitoredStatus(_artist, null);

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.IsAny<Author>()), Times.Once());

            Mocker.GetMock<IAlbumService>()
                  .Verify(v => v.UpdateMany(It.IsAny<List<Book>>()), Times.Never());
        }

        [Test]
        public void should_be_able_to_monitor_albums_when_passed_in_artist()
        {
            var albumsToMonitor = new List<string> { _albums.First().ForeignBookId };

            Subject.SetAlbumMonitoredStatus(_artist, new MonitoringOptions { Monitored = true, AlbumsToMonitor = albumsToMonitor });

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.IsAny<Author>()), Times.Once());

            VerifyMonitored(e => e.ForeignBookId == _albums.First().ForeignBookId);
            VerifyNotMonitored(e => e.ForeignBookId != _albums.First().ForeignBookId);
        }

        [Test]
        public void should_be_able_to_monitor_all_albums()
        {
            Subject.SetAlbumMonitoredStatus(_artist, new MonitoringOptions { Monitor = MonitorTypes.All });

            Mocker.GetMock<IAlbumService>()
                  .Verify(v => v.UpdateMany(It.Is<List<Book>>(l => l.All(e => e.Monitored))));
        }

        [Test]
        public void should_be_able_to_monitor_new_albums_only()
        {
            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.Future
            };

            Subject.SetAlbumMonitoredStatus(_artist, monitoringOptions);

            VerifyMonitored(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value.After(DateTime.UtcNow));
            VerifyMonitored(e => !e.ReleaseDate.HasValue);
            VerifyNotMonitored(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value.Before(DateTime.UtcNow));
        }

        private void VerifyMonitored(Func<Book, bool> predicate)
        {
            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Book>>(l => l.Where(predicate).All(e => e.Monitored))));
        }

        private void VerifyNotMonitored(Func<Book, bool> predicate)
        {
            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Book>>(l => l.Where(predicate).All(e => !e.Monitored))));
        }
    }
}
