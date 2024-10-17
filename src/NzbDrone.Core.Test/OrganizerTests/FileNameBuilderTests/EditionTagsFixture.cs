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

        [Test]
        [TestCase("10th anniversary edition", "{Movie Title} [{edition tags}]", "South Park [10th anniversary edition]")]
        [TestCase("10TH anniversary edition", "{Movie Title} [{edition tags}]", "South Park [10th anniversary edition]")]
        [TestCase("10Th anniversary edition", "{Movie Title} [{edition tags}]", "South Park [10th anniversary edition]")]
        [TestCase("10th anniversary edition", "{Movie Title} [{Edition Tags}]", "South Park [10th Anniversary Edition]")]
        [TestCase("10TH anniversary edition", "{Movie Title} [{Edition Tags}]", "South Park [10th Anniversary Edition]")]
        [TestCase("10Th anniversary edition", "{Movie Title} [{Edition Tags}]", "South Park [10th Anniversary Edition]")]
        [TestCase("10th anniversary edition", "{Movie Title} [{EDITION TAGS}]", "South Park [10TH ANNIVERSARY EDITION]")]
        [TestCase("10TH anniversary edition", "{Movie Title} [{EDITION TAGS}]", "South Park [10TH ANNIVERSARY EDITION]")]
        [TestCase("10Th anniversary edition", "{Movie Title} [{EDITION TAGS}]", "South Park [10TH ANNIVERSARY EDITION]")]
        public void should_always_lowercase_ordinals(string edition, string movieFormat, string expected)
        {
            _movieFile.Edition = edition;
            _namingConfig.StandardMovieFormat = movieFormat;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expected);
        }

        [Test]
        [TestCase("imax", "{Movie Title} [{edition tags}]", "South Park [imax]")]
        [TestCase("IMAX", "{Movie Title} [{edition tags}]", "South Park [imax]")]
        [TestCase("Imax", "{Movie Title} [{edition tags}]", "South Park [imax]")]
        [TestCase("imax", "{Movie Title} [{Edition Tags}]", "South Park [IMAX]")]
        [TestCase("IMAX", "{Movie Title} [{Edition Tags}]", "South Park [IMAX]")]
        [TestCase("Imax", "{Movie Title} [{Edition Tags}]", "South Park [IMAX]")]
        [TestCase("imax", "{Movie Title} [{EDITION TAGS}]", "South Park [IMAX]")]
        [TestCase("IMAX", "{Movie Title} [{EDITION TAGS}]", "South Park [IMAX]")]
        [TestCase("Imax", "{Movie Title} [{EDITION TAGS}]", "South Park [IMAX]")]

        [TestCase("3d", "{Movie Title} [{edition tags}]", "South Park [3d]")]
        [TestCase("3D", "{Movie Title} [{edition tags}]", "South Park [3d]")]
        [TestCase("3d", "{Movie Title} [{Edition Tags}]", "South Park [3D]")]
        [TestCase("3D", "{Movie Title} [{Edition Tags}]", "South Park [3D]")]
        [TestCase("3d", "{Movie Title} [{EDITION TAGS}]", "South Park [3D]")]
        [TestCase("3D", "{Movie Title} [{EDITION TAGS}]", "South Park [3D]")]

        [TestCase("hdr", "{Movie Title} [{edition tags}]", "South Park [hdr]")]
        [TestCase("HDR", "{Movie Title} [{edition tags}]", "South Park [hdr]")]
        [TestCase("Hdr", "{Movie Title} [{edition tags}]", "South Park [hdr]")]
        [TestCase("hdr", "{Movie Title} [{Edition Tags}]", "South Park [HDR]")]
        [TestCase("HDR", "{Movie Title} [{Edition Tags}]", "South Park [HDR]")]
        [TestCase("Hdr", "{Movie Title} [{Edition Tags}]", "South Park [HDR]")]
        [TestCase("hdr", "{Movie Title} [{EDITION TAGS}]", "South Park [HDR]")]
        [TestCase("HDR", "{Movie Title} [{EDITION TAGS}]", "South Park [HDR]")]
        [TestCase("Hdr", "{Movie Title} [{EDITION TAGS}]", "South Park [HDR]")]

        [TestCase("sdr", "{Movie Title} [{edition tags}]", "South Park [sdr]")]
        [TestCase("SDR", "{Movie Title} [{edition tags}]", "South Park [sdr]")]
        [TestCase("Sdr", "{Movie Title} [{edition tags}]", "South Park [sdr]")]
        [TestCase("sdr", "{Movie Title} [{Edition Tags}]", "South Park [SDR]")]
        [TestCase("SDR", "{Movie Title} [{Edition Tags}]", "South Park [SDR]")]
        [TestCase("Sdr", "{Movie Title} [{Edition Tags}]", "South Park [SDR]")]
        [TestCase("sdr", "{Movie Title} [{EDITION TAGS}]", "South Park [SDR]")]
        [TestCase("SDR", "{Movie Title} [{EDITION TAGS}]", "South Park [SDR]")]
        [TestCase("Sdr", "{Movie Title} [{EDITION TAGS}]", "South Park [SDR]")]

        public void should_always_uppercase_special_strings(string edition, string movieFormat, string expected)
        {
            _movieFile.Edition = edition;
            _namingConfig.StandardMovieFormat = movieFormat;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expected);
        }
    }
}
