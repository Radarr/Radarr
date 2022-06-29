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
    public class EditionTagsFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                .CreateNew()
                .With(s => s.Title = "South Park")
                .Build();

            _movieFile = new MovieFile { Quality = new QualityModel(), ReleaseGroup = "SonarrTest" };

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
        public void should_add_edition_tag()
        {
            _movieFile.Edition = "Uncut";
            _namingConfig.StandardMovieFormat = "{Movie Title} [{Edition Tags}]";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park [Uncut]");
        }

        [TestCase("{Movie Title} {edition-{Edition Tags}}")]
        public void should_conditional_hide_edition_tags_in_plex_format(string movieFormat)
        {
            _movieFile.Edition = "";
            _namingConfig.StandardMovieFormat = movieFormat;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park");
        }
    }
}
