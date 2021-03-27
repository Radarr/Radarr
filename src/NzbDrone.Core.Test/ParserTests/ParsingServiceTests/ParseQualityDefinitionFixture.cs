using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class ParseQualityDefinitionFixture : TestBase<ParsingService>
    {
        /*public Movie _movie;
        public IIndexerSettings _multiSettings;
        public IIndexerSettings _notMultiSettings;
        public ReleaseInfo _multiRelease;
        public ReleaseInfo _nonMultiRelease;
        public ParsedMovieInfo _webdlMovie;
        public ParsedMovieInfo _remuxMovie;
        public ParsedMovieInfo _remuxSurroundMovie;
        public ParsedMovieInfo _unknownMovie;

        [SetUp]
        public void Setup()
        {
            QualityDefinitionServiceFixture.SetupDefaultDefinitions();
            _movie = Builder<Movie>.CreateNew().Build();
            _multiSettings = Builder<NewznabSettings>.CreateNew()
                .With(s => s.MultiLanguages = new List<int>{(int)Language.English, (int)Language.French}).Build();
            _notMultiSettings = Builder<NewznabSettings>.CreateNew().Build();

            _multiRelease = Builder<ReleaseInfo>.CreateNew().With(r => r.Title = "My German Movie 2017 MULTI")
                .With(r => r.IndexerSettings = _multiSettings).Build();

            _nonMultiRelease = Builder<ReleaseInfo>.CreateNew().With(r => r.Title = "My Movie 2017")
                .With(r => r.IndexerSettings = _notMultiSettings).Build();

            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.All()).Returns(QualityDefinition.DefaultQualityDefinitions.ToList());

            Mocker.GetMock<IConfigService>().Setup(s => s.ParsingLeniency).Returns(ParsingLeniencyType.Strict);
        }

        private ParsedMovieInfo CopyWithInfo(ParsedMovieInfo existingInfo, params object[] info)
        {
            var dict = new Dictionary<string, object>();
            for (var i = 0; i < info.Length; i += 2) {
                dict.Add(info[i].ToString(), info[i+1]);
            }

            var newInfo = new ParsedMovieInfo
            {
                Edition = existingInfo.Edition,
                MovieTitle = existingInfo.MovieTitle,
                Quality = new QualityModel
                {
                    Resolution = existingInfo.Quality.Resolution,
                    Source = existingInfo.Quality.Source
                },
                Year = existingInfo.Year
            };
            newInfo.ExtraInfo = dict;
            return newInfo;
        }

        public void GivenExtraQD(QualityDefinition definition)
        {
            var defaults = QualityDefinition.DefaultQualityDefinitions.ToList();
            defaults.Add(definition);
            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.All()).Returns(defaults);
        }

        private void GivenExtraQD(params QualityDefinition[] definition)
        {
            var defaults = QualityDefinition.DefaultQualityDefinitions.ToList();
            defaults.AddRange(definition);
            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.All()).Returns(defaults);
        }

         TODO: Add quality definition integration tests?
        [TestCase("Movie 2017 Bluray 1080p", "Bluray-1080p")]
        [TestCase("Movie 2017 Bluray Remux 1080p", "Remux-1080p")]
        [TestCase("27.Movie.2008.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N", "Remux-1080p")]
        [TestCase("The.Movie.Movie.2016.03.14.1080p.WEB.h264-spamTV", "WEBDL-1080p")]
        public void should_correctly_identify_default_definition(string title, string definitionName)
        {
            var result = Subject.ParseMovieInfo(title, new List<object>());
            result.Quality.QualityDefinition.Title.Should().Be(definitionName);
        }

        [Test]
        public void should_correctly_choose_matching_filesize()
        {
            GivenExtraQD(new QualityDefinition
            {
                Title = "Small Bluray 1080p",
                QualityTags = new List<FormatTag>
                {
                    new FormatTag("s_bluray"),
                    new FormatTag("R_1080")
                },
                MaxSize = 50,
                MinSize = 0,
            }, new QualityDefinition
            {
                Title = "Small WEB 1080p",
                QualityTags = new List<FormatTag>
                {
                    new FormatTag("s_webdl"),
                    new FormatTag("R_1080")
                },
                MaxSize = 50,
                MinSize = 0,
            });
            var movieInfo = new ParsedMovieInfo
            {
                Edition = "",
                MovieTitle = "A Movie",
                Quality = new QualityModel
                {
                    Resolution = Resolution.R1080P,
                    Source = Source.BLURAY
                },
                Year = 2018
            };
            var webInfo = new ParsedMovieInfo
            {
                Edition = "",
                MovieTitle = "A Movie",
                Quality = new QualityModel
                {
                    Resolution = Resolution.R1080P,
                    Source = Source.WEBDL
                },
                Year = 2018
            };

            var smallSize = 2875.Megabytes(); //2.8GB
            var largeSize = 8625.Megabytes(); //8.6GB
            var largestSize = 20000.Megabytes(); //20GB

            Subject.ParseQualityDefinition(CopyWithInfo(movieInfo, "Size", smallSize)).Title.Should().Be("Small Bluray 1080p");
            Subject.ParseQualityDefinition(CopyWithInfo(movieInfo, "Size", largeSize)).Title.Should().Be("Bluray-1080p");
            Subject.ParseQualityDefinition(CopyWithInfo(movieInfo, "Size", largestSize)).Title.Should().Be("Bluray-1080p");
            Subject.ParseQualityDefinition(CopyWithInfo(webInfo, "Size", smallSize)).Title.Should().Be("Small WEB 1080p");
            Subject.ParseQualityDefinition(CopyWithInfo(webInfo, "Size", largeSize)).Title.Should().Be("WEBDL-1080p");
            Subject.ParseQualityDefinition(CopyWithInfo(webInfo, "Size", largestSize)).Title.Should().Be("WEBDL-1080p");
        }

        [TestCase("Movie.Title.Directors.Cut.2017.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N",
            "Remux-1080p Director")]
        [TestCase("Movie.Title.Directors.Edition.2017.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N",
            "Remux-1080p Director")]
        [TestCase("Movie.Title.2017.Directors.Edition.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N",
            "Remux-1080p Director")]
        [TestCase("Movie.Title.2017.Extended.Edition.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N",
            "Remux-1080p")]
        [TestCase("Movie.Title.2017.BDREMUX.1080p.Bluray.MULTI.French.English", "Remux-1080p FR")]
        [TestCase("Movie.Title.2017.BDREMUX.1080p.Bluray.French", "Remux-1080p FR")]
        [TestCase("Movie.Title.2017.BDREMUX.1080p.Bluray.English", "Remux-1080p")]
        [Test]
        public void should_correctly_identify_advanced_definitons()
        {
            GivenExtraQD(
                new QualityDefinition
                {
                    Title = "Remux-1080p Director",
                    QualityTags = new List<FormatTag>
                    {
                        new FormatTag("s_bluray"),
                        new FormatTag("R_1080"),
                        new FormatTag("m_remux"),
                        new FormatTag("e_director")
                    }
                },
                new QualityDefinition
                {
                    Title = "Remux-1080p FR",
                    QualityTags = new List<FormatTag>
                    {
                        new FormatTag("s_bluray"),
                        new FormatTag("R_1080"),
                        new FormatTag("m_remux"),
                        new FormatTag("l_re_french"),
                        new FormatTag("l_english")
                    }
                }
            );



            var result = Subject.ParseMovieInfo(title, new List<object>());
            result.Quality.QualityDefinition.Title.Should().Be(definitionName);
        }


        [TestCase("My Movie 2017 German English", Language.English, Language.German)]
        //[TestCase("Movie.2016.MULTi.1080p.BluRay.x264-ANONA", Language.English, Language.French)] fails since no mention of french!
        [TestCase("Movie (2016) MULTi VFQ [1080p] BluRay x264-PopHD", Language.English, Language.French)]
        [TestCase("Movie.2009.S01E14.Germany.HDTV.XviD-LOL", Language.English)]
        [TestCase("Movie.2009.S01E14.HDTV.XviD-LOL", Language.English)]
        [TestCase("The Danish Movie 2015", Language.English)]
        public void should_parse_advanced_languages_correctly(string title, params Language[] languages)
        {
            var result = Subject.ParseMovieInfo(title, new List<object>());
            result.Languages.Should().BeEquivalentTo(languages);
        }*/
    }
}
