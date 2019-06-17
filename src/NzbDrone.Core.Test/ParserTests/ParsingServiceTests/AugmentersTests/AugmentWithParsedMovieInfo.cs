using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.AugmentersTests
{
    [TestFixture]
    public class AugmentWithParsedMovieInfoFixture : AugmentMovieInfoFixture<AugmentWithParsedMovieInfo>
    {
        [Test]
        public void should_add_edition_if_null()
        {
            var folderInfo = new ParsedMovieInfo
            {
                Edition = "Directors Cut"
            };

            var result = Subject.AugmentMovieInfo(MovieInfo, folderInfo);

            result.Edition.Should().Be(folderInfo.Edition);
        }

        [Test]
        public void should_preferr_longer_edition()
        {
            var folderInfo = new ParsedMovieInfo
            {
                Edition = "Super duper cut"
            };

            MovieInfo.Edition = "Rogue";

            var result = Subject.AugmentMovieInfo(MovieInfo, folderInfo);

            result.Edition.Should().Be(folderInfo.Edition);

            MovieInfo.Edition = "Super duper awesome cut";

            result = Subject.AugmentMovieInfo(MovieInfo, folderInfo);

            result.Edition.Should().Be(MovieInfo.Edition);
        }

        [Test]
        public void should_combine_languages()
        {
            var folderInfo = new ParsedMovieInfo
            {
                Languages = new List<Language> {Language.French}
            };

            MovieInfo.Languages = new List<Language>{Language.English};

            var result = Subject.AugmentMovieInfo(MovieInfo, folderInfo);

            result.Languages.Should().BeEquivalentTo(Language.English, Language.French);
        }

        [Test]
        public void should_combine_formats()
        {
            var folderInfo = new ParsedMovieInfo
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            var format1 = new CustomFormats.CustomFormat("Awesome Format");
            format1.Id = 1;

            var format2 = new CustomFormats.CustomFormat("Cool Format");
            format2.Id = 2;

            folderInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> { format1 };

            MovieInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> { format2 };

            var result = Subject.AugmentMovieInfo(MovieInfo, folderInfo);

            result.Quality.CustomFormats.Count.Should().Be(2);
            result.Quality.CustomFormats.Should().BeEquivalentTo(format2, format1);

            folderInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> { format1, format2 };

            result = Subject.AugmentMovieInfo(MovieInfo, folderInfo);

            result.Quality.CustomFormats.Count.Should().Be(2);
            result.Quality.CustomFormats.Should().BeEquivalentTo(format2, format1);
        }

        [Test]
        public void should_use_folder_release_group()
        {
            var folderInfo = new ParsedMovieInfo
            {
                ReleaseGroup = "AwesomeGroup"
            };

            MovieInfo.ReleaseGroup = "";

            var result = Subject.AugmentMovieInfo(MovieInfo, folderInfo);

            result.ReleaseGroup.Should().BeEquivalentTo(folderInfo.ReleaseGroup);
        }
    }
}
