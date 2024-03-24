using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.movieImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateSubtitleInfoFixture : CoreTest<AggregateSubtitleInfo>
    {
        [TestCase("Name (2020)/Name (2020) - [AAC 2.0].mkv", "", "Name (2020) - [AAC 2.0].default.eng.forced.ass", null)]
        [TestCase("Name (2020)/Name (2020) - [AAC 2.0].mkv", "", "Name (2020) - [AAC 2.0].eng.default.ass", null)]
        [TestCase("Name (2020)/Name (2020) - [AAC 2.0].mkv", "", "Name (2020) - [AAC 2.0].fra.ass", null)]
        [TestCase("", "Name (2020)/Name (2020) - [AAC 2.0].mkv", "Name (2020) - [AAC 2.0].default.eng.forced.ass", "Name (2020)/Name (2020) - [FLAC 2.0].mkv")]
        [TestCase("", "Name (2020)/Name (2020) - [AAC 2.0].mkv", "Name (2020) - [AAC 2.0].eng.default.ass", null)]
        [TestCase("", "Name (2020)/Name (2020) - [AAC 2.0].mkv", "Name (2020) - [AAC 2.0].fra.ass", null)]
        public void should_do_basic_parse(string relativePath, string originalFilePath, string path, string fileNameBeforeRename)
        {
            var movieFile = new MovieFile
            {
                RelativePath = relativePath,
                OriginalFilePath = originalFilePath
            };

            var subtitleTitleInfo = Subject.CleanSubtitleTitleInfo(movieFile, path, fileNameBeforeRename);

            subtitleTitleInfo.Title.Should().BeNull();
            subtitleTitleInfo.Copy.Should().Be(0);
        }

        [TestCase("Default (2020)/Default (2020) - [AAC 2.0].mkv", "Default (2020) - [AAC 2.0].default.eng.forced.ass")]
        [TestCase("Default (2020)/Default (2020) - [AAC 2.0].mkv", "Default (2020) - [AAC 2.0].eng.default.ass")]
        [TestCase("Default (2020)/Default (2020) - [AAC 2.0].mkv", "Default (2020) - [AAC 2.0].default.eng.testtitle.forced.ass")]
        [TestCase("Default (2020)/Default (2020) - [AAC 2.0].mkv", "Default (2020) - [AAC 2.0].testtitle.eng.default.ass")]
        public void should_not_parse_default(string relativePath, string path)
        {
            var movieFile = new MovieFile
            {
                RelativePath = relativePath
            };

            var subtitleTitleInfo = Subject.CleanSubtitleTitleInfo(movieFile, path, null);

            subtitleTitleInfo.LanguageTags.Should().NotContain("default");
        }
    }
}
