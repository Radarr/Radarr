using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateReleaseGroupFixture : CoreTest<AggregateReleaseGroup>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew().Build();
        }

        [Test]
        public void should_prefer_downloadclient()
        {
            var fileMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL-Wizzy", false);
            var folderMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL-Drone", false);
            var downloadClientMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL-Viva", false);
            var localMovie = new LocalMovie
            {
                FileMovieInfo = fileMovieInfo,
                FolderMovieInfo = folderMovieInfo,
                DownloadClientMovieInfo = downloadClientMovieInfo,
                Path = @"C:\Test\Unsorted Movies\Movie.Title.2008\Movie.Title.2008.WEB-DL.mkv".AsOsAgnostic(),
                Movie = _movie
            };

            Subject.Aggregate(localMovie, null, false);

            localMovie.ReleaseGroup.Should().Be("Viva");
        }

        [Test]
        public void should_prefer_folder()
        {
            var fileMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL-Wizzy", false);
            var folderMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL-Drone", false);
            var downloadClientMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL", false);
            var localMovie = new LocalMovie
            {
                FileMovieInfo = fileMovieInfo,
                FolderMovieInfo = folderMovieInfo,
                DownloadClientMovieInfo = downloadClientMovieInfo,
                Path = @"C:\Test\Unsorted Movies\Movie.Title.2008\Movie.Title.2008.WEB-DL.mkv".AsOsAgnostic(),
                Movie = _movie
            };

            Subject.Aggregate(localMovie, null, false);

            localMovie.ReleaseGroup.Should().Be("Drone");
        }

        [Test]
        public void should_fallback_to_file()
        {
            var fileMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL-Wizzy", false);
            var folderMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL", false);
            var downloadClientMovieInfo = Parser.Parser.ParseMovieTitle("Movie.Title.2008.WEB-DL", false);
            var localMovie = new LocalMovie
            {
                FileMovieInfo = fileMovieInfo,
                FolderMovieInfo = folderMovieInfo,
                DownloadClientMovieInfo = downloadClientMovieInfo,
                Path = @"C:\Test\Unsorted Movies\Movie.Title.2008\Movie.Title.2008.mkv".AsOsAgnostic(),
                Movie = _movie
            };

            Subject.Aggregate(localMovie, null, false);

            localMovie.ReleaseGroup.Should().Be("Wizzy");
        }
    }
}
