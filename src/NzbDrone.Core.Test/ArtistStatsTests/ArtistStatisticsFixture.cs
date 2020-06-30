using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ArtistStatsTests
{
    [TestFixture]
    public class ArtistStatisticsFixture : DbTest<AuthorStatisticsRepository, Author>
    {
        private Author _artist;
        private Book _album;
        private Edition _edition;
        private BookFile _trackFile;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Author>.CreateNew()
                .With(a => a.AuthorMetadataId = 10)
                .BuildNew();
            Db.Insert(_artist);

            _album = Builder<Book>.CreateNew()
                .With(e => e.ReleaseDate = DateTime.Today.AddDays(-5))
                .With(e => e.AuthorMetadataId = 10)
                .BuildNew();
            Db.Insert(_album);

            _edition = Builder<Edition>.CreateNew()
                .With(e => e.BookId = _album.Id)
                .With(e => e.Monitored = true)
                .BuildNew();
            Db.Insert(_edition);

            _trackFile = Builder<BookFile>.CreateNew()
                .With(e => e.Author = _artist)
                .With(e => e.Edition = _edition)
                .With(e => e.EditionId == _edition.Id)
                .With(e => e.Quality = new QualityModel(Quality.MP3_320))
                .BuildNew();
        }

        private void GivenTrackFile()
        {
            Db.Insert(_trackFile);
        }

        [Test]
        public void should_get_stats_for_artist()
        {
            var stats = Subject.AuthorStatistics();

            stats.Should().HaveCount(1);
        }

        [Test]
        public void should_not_include_unmonitored_track_in_track_count()
        {
            var stats = Subject.AuthorStatistics();

            stats.Should().HaveCount(1);
            stats.First().BookCount.Should().Be(0);
        }

        [Test]
        public void should_include_unmonitored_track_with_file_in_track_count()
        {
            GivenTrackFile();

            var stats = Subject.AuthorStatistics();

            stats.Should().HaveCount(1);
            stats.First().BookCount.Should().Be(1);
        }

        [Test]
        public void should_have_size_on_disk_of_zero_when_no_track_file()
        {
            var stats = Subject.AuthorStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(0);
        }

        [Test]
        public void should_have_size_on_disk_when_track_file_exists()
        {
            GivenTrackFile();

            var stats = Subject.AuthorStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(_trackFile.Size);
        }
    }
}
