using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class OriginalTitleFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                    .CreateNew()
                    .With(s => s.Title = "My Movie")
                    .Build();

            _movieFile = new MovieFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RadarrTest" };

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameMovies = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                .Setup(v => v.All())
                .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_not_recursively_include_current_filename()
        {
            _movieFile.RelativePath = "My Movie";
            _namingConfig.StandardMovieFormat = "{Movie Title} {[Original Title]}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("My Movie");
        }

        [Test]
        public void should_include_original_title_if_not_current_file_name()
        {
            _movieFile.SceneName = "my.movie.2008";
            _movieFile.RelativePath = "My Movie";
            _namingConfig.StandardMovieFormat = "{Movie Title} {[Original Title]}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("My Movie [my.movie.2008]");
        }

        [Test]
        public void should_include_current_filename_if_not_renaming_files()
        {
            _movieFile.SceneName = "my.movie.2008";
            _namingConfig.RenameMovies = false;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("my.movie.2008");
        }

        [Test]
        public void should_include_current_filename_if_not_including_multiple_naming_tokens()
        {
            _movieFile.RelativePath = "My Movie";
            _namingConfig.StandardMovieFormat = "{Original Title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("My Movie");
        }
    }
}
