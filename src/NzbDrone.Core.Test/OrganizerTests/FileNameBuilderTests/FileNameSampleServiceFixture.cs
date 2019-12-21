using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class FileNameSampleServiceFixture : CoreTest<FileNameSampleService>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(c => c.BuildFileName(It.IsAny<Movie>(), It.IsAny<MovieFile>(), _namingConfig, null)).Returns("Movie");

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(c => c.GetMovieFolder(It.IsAny<Movie>(), _namingConfig)).Returns("Movie (2019)");
        }

        private void ThrowNamingException()
        {
            Mocker.GetMock<IBuildFileNames>()
                .Setup(c => c.BuildFileName(It.IsAny<Movie>(), It.IsAny<MovieFile>(), _namingConfig, null))
                .Throws(new NamingFormatException("Invalid Naming Format"));
        }

        [Test]
        public void should_get_movie_sample()
        {
            var sampleResult = Subject.GetMovieSample(_namingConfig);

            sampleResult.Should().NotBeNull();
            sampleResult.Movie.Should().BeNull();
            sampleResult.MovieFile.Should().BeNull();
            sampleResult.FileName.Should().Be("Movie");
        }

        [Test]
        public void should_get_movie_folder_sample()
        {
            var sampleResult = Subject.GetMovieFolderSample(_namingConfig);

            sampleResult.Should().NotBeNull();
            sampleResult.Should().Be("Movie (2019)");
        }

        [Test]
        public void should_handle_naming_format_exception()
        {
            ThrowNamingException();

            var sampleResult = Subject.GetMovieSample(_namingConfig);

            ExceptionVerification.ExpectedErrors(0);

            sampleResult.Should().NotBeNull();
            sampleResult.Movie.Should().BeNull();
            sampleResult.MovieFile.Should().BeNull();
            sampleResult.FileName.Should().BeEmpty();
        }
    }
}
