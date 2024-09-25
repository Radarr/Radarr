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
    public class ColonReplacementFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                    .CreateNew()
                    .With(s => s.Title = "CSI: Vegas")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameMovies = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _movieFile = new MovieFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RadarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_replace_colon_followed_by_space_with_space_dash_space_by_default()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("CSI - Vegas");
        }

        [TestCase("CSI: Vegas", ColonReplacementFormat.Smart, "CSI - Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.Dash, "CSI- Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.Delete, "CSI Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.SpaceDash, "CSI - Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.SpaceDashSpace, "CSI - Vegas")]
        public void should_replace_colon_followed_by_space_with_expected_result(string movieName, ColonReplacementFormat replacementFormat, string expected)
        {
            _movie.Title = movieName;
            _namingConfig.StandardMovieFormat = "{Movie Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;

            Subject.BuildFileName(_movie, _movieFile)
                .Should().Be(expected);
        }

        [TestCase("Movie:Title", ColonReplacementFormat.Smart, "Movie-Title")]
        [TestCase("Movie:Title", ColonReplacementFormat.Dash, "Movie-Title")]
        [TestCase("Movie:Title", ColonReplacementFormat.Delete, "MovieTitle")]
        [TestCase("Movie:Title", ColonReplacementFormat.SpaceDash, "Movie -Title")]
        [TestCase("Movie:Title", ColonReplacementFormat.SpaceDashSpace, "Movie - Title")]
        public void should_replace_colon_with_expected_result(string movieName, ColonReplacementFormat replacementFormat, string expected)
        {
            _movie.Title = movieName;
            _namingConfig.StandardMovieFormat = "{Movie Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;

            Subject.BuildFileName(_movie, _movieFile)
                .Should().Be(expected);
        }
    }
}
