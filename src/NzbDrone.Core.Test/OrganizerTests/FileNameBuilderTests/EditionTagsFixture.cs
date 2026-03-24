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
                .With(m => m.Title = "Movie Title")
                .Build();

            _movieFile = new MovieFile { Quality = new QualityModel(), ReleaseGroup = "RadarrTest", Edition = "Uncut" };

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
            _namingConfig.StandardMovieFormat = "{Movie Title} [{Edition Tags}]";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("Movie Title [Uncut]");
        }

        [TestCase("{Movie Title} {Edition Tags}")]
        [TestCase("{Movie Title} {{Edition Tags}}")]
        [TestCase("{Movie Title} {edition-{Edition Tags}}")]
        [TestCase("{Movie Title} {{edition-{Edition Tags}}}")]
        public void should_conditional_hide_edition_tags(string movieFormat)
        {
            _movieFile.Edition = "";
            _namingConfig.StandardMovieFormat = movieFormat;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("Movie Title");
        }

        [TestCase("{Movie Title} {{Edition Tags}}")]
        public void should_handle_edition_curly_brackets(string movieFormat)
        {
            _namingConfig.StandardMovieFormat = movieFormat;

            Subject.BuildFileName(_movie, _movieFile)
                .Should().Be("Movie Title {Uncut}");
        }

        [TestCase("{Movie Title} {{edition-{Edition Tags}}}")]
        public void should_handle_edition_tag_curly_brackets(string movieFormat)
        {
            _namingConfig.StandardMovieFormat = movieFormat;

            Subject.BuildFileName(_movie, _movieFile)
                .Should().Be("Movie Title {{edition-Uncut}}");
        }

        [TestCase("1st anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [1st Anniversary Edition]")]
        [TestCase("2nd Anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [2nd Anniversary Edition]")]
        [TestCase("3rd anniversary Edition", "{Movie Title} [{Edition Tags}]", "Movie Title [3rd Anniversary Edition]")]
        [TestCase("4th anNiverSary eDitIOn", "{Movie Title} [{Edition Tags}]", "Movie Title [4th Anniversary Edition]")]
        [TestCase("5th anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [5th Anniversary Edition]")]
        [TestCase("6th anNiverSary EDITION", "{Movie Title} [{Edition Tags}]", "Movie Title [6th Anniversary Edition]")]
        [TestCase("7TH anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [7th Anniversary Edition]")]
        [TestCase("8Th anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [8th Anniversary Edition]")]
        [TestCase("9tH anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [9th Anniversary Edition]")]
        [TestCase("10th anniversary edition", "{Movie Title} [{edition tags}]", "Movie Title [10th anniversary edition]")]
        [TestCase("10TH anniversary edition", "{Movie Title} [{edition tags}]", "Movie Title [10th anniversary edition]")]
        [TestCase("10Th anniversary edition", "{Movie Title} [{edition tags}]", "Movie Title [10th anniversary edition]")]
        [TestCase("10th anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [10th Anniversary Edition]")]
        [TestCase("10TH anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [10th Anniversary Edition]")]
        [TestCase("10Th anniversary edition", "{Movie Title} [{Edition Tags}]", "Movie Title [10th Anniversary Edition]")]
        [TestCase("10th anniversary edition", "{Movie Title} [{EDITION TAGS}]", "Movie Title [10TH ANNIVERSARY EDITION]")]
        [TestCase("10TH anniversary edition", "{Movie Title} [{EDITION TAGS}]", "Movie Title [10TH ANNIVERSARY EDITION]")]
        [TestCase("10Th anniversary edition", "{Movie Title} [{EDITION TAGS}]", "Movie Title [10TH ANNIVERSARY EDITION]")]
        public void should_always_lowercase_ordinals(string edition, string movieFormat, string expected)
        {
            _movieFile.Edition = edition;
            _namingConfig.StandardMovieFormat = movieFormat;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expected);
        }

        [TestCase("imax", "{Movie Title} [{edition tags}]", "Movie Title [imax]")]
        [TestCase("IMAX", "{Movie Title} [{edition tags}]", "Movie Title [imax]")]
        [TestCase("Imax", "{Movie Title} [{edition tags}]", "Movie Title [imax]")]
        [TestCase("imax", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX]")]
        [TestCase("IMAX", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX]")]
        [TestCase("Imax", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX]")]
        [TestCase("imax", "{Movie Title} [{EDITION TAGS}]", "Movie Title [IMAX]")]
        [TestCase("IMAX", "{Movie Title} [{EDITION TAGS}]", "Movie Title [IMAX]")]
        [TestCase("Imax", "{Movie Title} [{EDITION TAGS}]", "Movie Title [IMAX]")]
        [TestCase("imax edition", "{Movie Title} [{edition tags}]", "Movie Title [imax edition]")]
        [TestCase("imax edition", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX Edition]")]
        [TestCase("Imax edition", "{Movie Title} [{EDITION TAGS}]", "Movie Title [IMAX EDITION]")]
        [TestCase("imax version", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX Version]")]
        [TestCase("IMAX-edition", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX-Edition]")]
        [TestCase("IMAX_edition", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX_Edition]")]
        [TestCase("IMAX.eDiTioN", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX.Edition]")]
        [TestCase("IMAX ed.", "{Movie Title} [{edition tags}]", "Movie Title [imax ed.]")]
        [TestCase("IMAX ed.", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX Ed.]")]
        [TestCase("Imax-ed.", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX-Ed.]")]
        [TestCase("imax.Ed", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX.Ed]")]
        [TestCase("Imax_ed", "{Movie Title} [{Edition Tags}]", "Movie Title [IMAX_Ed]")]
        [TestCase("3d", "{Movie Title} [{edition tags}]", "Movie Title [3d]")]
        [TestCase("3D", "{Movie Title} [{edition tags}]", "Movie Title [3d]")]
        [TestCase("3d", "{Movie Title} [{Edition Tags}]", "Movie Title [3D]")]
        [TestCase("3D", "{Movie Title} [{Edition Tags}]", "Movie Title [3D]")]
        [TestCase("3d", "{Movie Title} [{EDITION TAGS}]", "Movie Title [3D]")]
        [TestCase("3D", "{Movie Title} [{EDITION TAGS}]", "Movie Title [3D]")]
        [TestCase("hdr", "{Movie Title} [{edition tags}]", "Movie Title [hdr]")]
        [TestCase("HDR", "{Movie Title} [{edition tags}]", "Movie Title [hdr]")]
        [TestCase("Hdr", "{Movie Title} [{edition tags}]", "Movie Title [hdr]")]
        [TestCase("hdr", "{Movie Title} [{Edition Tags}]", "Movie Title [HDR]")]
        [TestCase("HDR", "{Movie Title} [{Edition Tags}]", "Movie Title [HDR]")]
        [TestCase("Hdr", "{Movie Title} [{Edition Tags}]", "Movie Title [HDR]")]
        [TestCase("hdr", "{Movie Title} [{EDITION TAGS}]", "Movie Title [HDR]")]
        [TestCase("HDR", "{Movie Title} [{EDITION TAGS}]", "Movie Title [HDR]")]
        [TestCase("Hdr", "{Movie Title} [{EDITION TAGS}]", "Movie Title [HDR]")]
        [TestCase("dv", "{Movie Title} [{edition tags}]", "Movie Title [dv]")]
        [TestCase("DV", "{Movie Title} [{edition tags}]", "Movie Title [dv]")]
        [TestCase("Dv", "{Movie Title} [{edition tags}]", "Movie Title [dv]")]
        [TestCase("dv", "{Movie Title} [{Edition Tags}]", "Movie Title [DV]")]
        [TestCase("DV", "{Movie Title} [{Edition Tags}]", "Movie Title [DV]")]
        [TestCase("Dv", "{Movie Title} [{Edition Tags}]", "Movie Title [DV]")]
        [TestCase("dv", "{Movie Title} [{EDITION TAGS}]", "Movie Title [DV]")]
        [TestCase("DV", "{Movie Title} [{EDITION TAGS}]", "Movie Title [DV]")]
        [TestCase("Dv", "{Movie Title} [{EDITION TAGS}]", "Movie Title [DV]")]
        [TestCase("sdr", "{Movie Title} [{edition tags}]", "Movie Title [sdr]")]
        [TestCase("SDR", "{Movie Title} [{edition tags}]", "Movie Title [sdr]")]
        [TestCase("Sdr", "{Movie Title} [{edition tags}]", "Movie Title [sdr]")]
        [TestCase("sdr", "{Movie Title} [{Edition Tags}]", "Movie Title [SDR]")]
        [TestCase("SDR", "{Movie Title} [{Edition Tags}]", "Movie Title [SDR]")]
        [TestCase("Sdr", "{Movie Title} [{Edition Tags}]", "Movie Title [SDR]")]
        [TestCase("sdr", "{Movie Title} [{EDITION TAGS}]", "Movie Title [SDR]")]
        [TestCase("SDR", "{Movie Title} [{EDITION TAGS}]", "Movie Title [SDR]")]
        [TestCase("Sdr", "{Movie Title} [{EDITION TAGS}]", "Movie Title [SDR]")]
        [TestCase("THEATRICAL", "{Movie Title} [{Edition Tags}]", "Movie Title [Theatrical]")]
        [TestCase("director's CUt", "{Movie Title} [{Edition Tags}]", "Movie Title [Director's Cut]")]
        public void should_always_uppercase_special_strings(string edition, string movieFormat, string expected)
        {
            _movieFile.Edition = edition;
            _namingConfig.StandardMovieFormat = movieFormat;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expected);
        }
    }
}
