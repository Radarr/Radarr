using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.AlbumMonitoredServiceTests
{
    [TestFixture]
    public class SetAlbumMontitoredFixture : CoreTest<AlbumMonitoredService>
    {
        private Artist _artist;
        private List<Album> _albums;

        [SetUp]
        public void Setup()
        {
            const int albums = 4;

            _artist = Builder<Artist>.CreateNew()
                                     .Build();

            _albums = Builder<Album>.CreateListOfSize(albums)
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
                .Setup(s => s.GetArtistAlbumsWithFiles(It.IsAny<Artist>()))
                .Returns(new List<Album>());

            Mocker.GetMock<ITrackService>()
                .Setup(s => s.GetTracksByAlbum(It.IsAny<int>()))
                .Returns(new List<Track>());
        }

        [Test]
        public void should_be_able_to_monitor_artist_without_changing_albums()
        {
            Subject.SetAlbumMonitoredStatus(_artist, null);

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.IsAny<Artist>()), Times.Once());

            Mocker.GetMock<IAlbumService>()
                  .Verify(v => v.UpdateMany(It.IsAny<List<Album>>()), Times.Never());
        }

        [Test]
        public void should_be_able_to_monitor_albums_when_passed_in_artist()
        {
            var albumsToMonitor = new List<string>{_albums.First().ForeignAlbumId};

            Subject.SetAlbumMonitoredStatus(_artist, new MonitoringOptions { Monitored = true, AlbumsToMonitor = albumsToMonitor });

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.IsAny<Artist>()), Times.Once());

            VerifyMonitored(e => e.ForeignAlbumId == _albums.First().ForeignAlbumId);
            VerifyNotMonitored(e => e.ForeignAlbumId != _albums.First().ForeignAlbumId);
        }

        [Test]
        public void should_be_able_to_monitor_all_albums()
        {
            Subject.SetAlbumMonitoredStatus(_artist, new MonitoringOptions{Monitor = MonitorTypes.All});

            Mocker.GetMock<IAlbumService>()
                  .Verify(v => v.UpdateMany(It.Is<List<Album>>(l => l.All(e => e.Monitored))));
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

        private void VerifyMonitored(Func<Album, bool> predicate)
        {
            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Album>>(l => l.Where(predicate).All(e => e.Monitored))));
        }

        private void VerifyNotMonitored(Func<Album, bool> predicate)
        {
            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Album>>(l => l.Where(predicate).All(e => !e.Monitored))));
        }
    }
}
