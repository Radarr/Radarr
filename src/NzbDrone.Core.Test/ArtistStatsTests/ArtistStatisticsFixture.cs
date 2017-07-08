using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.ArtistStatsTests
{
    [TestFixture]
    public class ArtistStatisticsFixture : DbTest<ArtistStatisticsRepository, Artist>
    {
        private Artist _artist;
        private Album _album;
        private Track _track;
        private TrackFile _trackFile;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                        .BuildNew();

            _album = Builder<Album>.CreateNew()
                                        .With(e => e.ReleaseDate = DateTime.Today.AddDays(5))
                                        .BuildNew();

            _artist.Id = Db.Insert(_artist).Id;
            _artist.Id = Db.Insert(_album).Id;

            _track = Builder<Track>.CreateNew()
                                          .With(e => e.TrackFileId = 0)
                                          .With(e => e.Monitored = false)
                                          .With(e => e.ArtistId = _artist.Id)
                                          .With(e => e.AlbumId = _album.Id)
                                          .BuildNew();

            _trackFile = Builder<TrackFile>.CreateNew()
                                           .With(e => e.ArtistId = _artist.Id)
                                           .With(e => e.AlbumId = _album.Id)
                                           .With(e => e.Quality = new QualityModel(Quality.MP3_256))
                                           .BuildNew();

        }

        private void GivenTrackWithFile()
        {
            _track.TrackFileId = 1;
        }

        private void GivenMonitoredTrack()
        {
            _track.Monitored = true;
        }

        private void GivenTrack()
        {
            Db.Insert(_track);
        }

        private void GivenTrackFile()
        {
            Db.Insert(_trackFile);
        }

        [Test]
        public void should_get_stats_for_artist()
        {
            GivenMonitoredTrack();
            GivenTrack();

            var stats = Subject.ArtistStatistics();

            stats.Should().HaveCount(1);
        }

        [Test]
        public void should_not_include_unmonitored_track_in_track_count()
        {
            GivenTrack();

            var stats = Subject.ArtistStatistics();

            stats.Should().HaveCount(1);
            stats.First().TrackCount.Should().Be(0);
        }

        [Test]
        public void should_include_unmonitored_track_with_file_in_track_count()
        {
            GivenTrackWithFile();
            GivenTrack();

            var stats = Subject.ArtistStatistics();

            stats.Should().HaveCount(1);
            stats.First().TrackCount.Should().Be(1);
        }

        [Test]
        public void should_have_size_on_disk_of_zero_when_no_track_file()
        {
            GivenTrack();

            var stats = Subject.ArtistStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(0);
        }

        [Test]
        public void should_have_size_on_disk_when_track_file_exists()
        {
            GivenTrack();
            GivenTrackFile();

            var stats = Subject.ArtistStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(_trackFile.Size);
        }

    }
}
