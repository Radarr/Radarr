using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MovieStats;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieStatsTests
{
    [TestFixture]
    public class MovieStatisticsFixture : DbTest<MovieStatisticsRepository, Movie>
    {
        private Movie _movie;
        private MovieFile _movieFile;

        [SetUp]
        public void Setup()
        {
            var movieMetadata = Builder<MovieMetadata>.CreateNew()
                .With(h => h.TmdbId = 123456)
                .With(m => m.Runtime = 90)
                .BuildNew();
            Db.Insert(movieMetadata);

            _movie = Builder<Movie>.CreateNew()
                .With(m => m.MovieMetadataId = movieMetadata.Id)
                .With(e => e.MovieFileId = 0)
                .With(e => e.Monitored = false)
                .BuildNew();

            _movie.Id = Db.Insert(_movie).Id;

            _movieFile = Builder<MovieFile>.CreateNew()
                .With(e => e.MovieId = _movie.Id)
                .With(e => e.Quality = new QualityModel(Quality.Bluray720p))
                .With(e => e.Languages = new List<Language> { Language.English })
                .BuildNew();
        }

        private void GivenMovieWithFile()
        {
            _movie.MovieFileId = 1;
        }

        private void GivenMonitoredMovie()
        {
            _movie.Monitored = true;
        }

        private void GivenMovieFile()
        {
            Db.Insert(_movieFile);
        }

        [Test]
        public void should_get_stats_for_movie()
        {
            GivenMonitoredMovie();

            var stats = Subject.MovieStatistics();

            stats.Should().HaveCount(1);
        }

        [Test]
        public void should_have_size_on_disk_of_zero_when_no_movie_file()
        {
            var stats = Subject.MovieStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(0);
        }

        [Test]
        public void should_have_size_on_disk_when_movie_file_exists()
        {
            GivenMovieWithFile();
            GivenMovieFile();

            var stats = Subject.MovieStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(_movieFile.Size);
        }

        // [Test]
        // public void should_not_duplicate_size_for_multi_movie_files()
        // {
        //     GivenMovieWithFile();
        //     GivenMovieFile();
        //
        //     var movie2 = _movie.JsonClone();
        //
        //     var movieMetadata = Builder<MovieMetadata>.CreateNew().With(h => h.TmdbId = 234567).BuildNew();
        //     Db.Insert(movieMetadata);
        //
        //     movie2.Id = 0;
        //     movie2.MovieMetadataId = movieMetadata.Id;
        //
        //     Db.Insert(movie2);
        //
        //     var stats = Subject.MovieStatistics();
        //
        //     stats.Should().HaveCount(1);
        //     stats.First().SizeOnDisk.Should().Be(_movieFile.Size);
        // }
    }
}
