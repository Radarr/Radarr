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

    public class CustomFormatsFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private NamingConfig _namingConfig;

        private List<CustomFormat> _customFormats;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameMovies = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _movieFile = new MovieFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RadarrTest" };

            _customFormats = new List<CustomFormat>()
            {
                new CustomFormat()
                {
                    Name = "INTERNAL",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat()
                {
                    Name = "AMZN",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat()
                {
                    Name = "NAME WITH SPACES",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat()
                {
                    Name = "NotIncludedFormat",
                    IncludeCustomFormatWhenRenaming = false
                }
            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [TestCase("{Custom Formats}", "INTERNAL AMZN NAME WITH SPACES")]
        public void should_replace_custom_formats(string format, string expected)
        {
            _namingConfig.StandardMovieFormat = format;

            Subject.BuildFileName(_movie, _movieFile, customFormats: _customFormats)
                   .Should().Be(expected);
        }

        [TestCase("{Custom Formats}", "")]
        public void should_replace_custom_formats_with_no_custom_formats(string format, string expected)
        {
            _namingConfig.StandardMovieFormat = format;

            Subject.BuildFileName(_movie, _movieFile, customFormats: new List<CustomFormat>())
                   .Should().Be(expected);
        }

        [TestCase("{Custom Formats:-INTERNAL}", "AMZN NAME WITH SPACES")]
        [TestCase("{Custom Formats:-NAME WITH SPACES}", "INTERNAL AMZN")]
        [TestCase("{Custom Formats:-INTERNAL,NAME WITH SPACES}", "AMZN")]
        [TestCase("{Custom Formats:INTERNAL}", "INTERNAL")]
        [TestCase("{Custom Formats:NAME WITH SPACES}", "NAME WITH SPACES")]
        [TestCase("{Custom Formats:INTERNAL,NAME WITH SPACES}", "INTERNAL NAME WITH SPACES")]
        public void should_replace_custom_formats_with_filtered_names(string format, string expected)
        {
            _namingConfig.StandardMovieFormat = format;

            Subject.BuildFileName(_movie, _movieFile, customFormats: _customFormats)
                   .Should().Be(expected);
        }

        [TestCase("{Custom Formats:-}", "{Custom Formats:-}")]
        [TestCase("{Custom Formats:}", "{Custom Formats:}")]
        public void should_not_replace_custom_formats_due_to_invalid_token(string format, string expected)
        {
            _namingConfig.StandardMovieFormat = format;

            Subject.BuildFileName(_movie, _movieFile, customFormats: _customFormats)
                   .Should().Be(expected);
        }

        [TestCase("{Custom Format}", "")]
        [TestCase("{Custom Format:INTERNAL}", "INTERNAL")]
        [TestCase("{Custom Format:AMZN}", "AMZN")]
        [TestCase("{Custom Format:NAME WITH SPACES}", "NAME WITH SPACES")]
        [TestCase("{Custom Format:DOESNOTEXIST}", "")]
        [TestCase("{Custom Format:INTERNAL} - {Custom Format:AMZN}", "INTERNAL - AMZN")]
        [TestCase("{Custom Format:AMZN} - {Custom Format:INTERNAL}", "AMZN - INTERNAL")]
        public void should_replace_custom_format(string format, string expected)
        {
            _namingConfig.StandardMovieFormat = format;

            Subject.BuildFileName(_movie, _movieFile, customFormats: _customFormats)
                   .Should().Be(expected);
        }

        [TestCase("{Custom Format}", "")]
        [TestCase("{Custom Format:INTERNAL}", "")]
        [TestCase("{Custom Format:AMZN}", "")]
        public void should_replace_custom_format_with_no_custom_formats(string format, string expected)
        {
            _namingConfig.StandardMovieFormat = format;

            Subject.BuildFileName(_movie, _movieFile, customFormats: new List<CustomFormat>())
                   .Should().Be(expected);
        }
    }
}
