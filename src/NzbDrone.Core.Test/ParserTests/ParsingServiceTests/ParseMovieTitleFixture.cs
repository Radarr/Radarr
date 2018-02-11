using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class ParseMovieTitleFixture : TestBase<ParsingService>
    {
        public Movie _movie;
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
            _movie = Builder<Movie>.CreateNew().Build();
            _multiSettings = Builder<NewznabSettings>.CreateNew()
                .With(s => s.MultiLanguages = new List<int>{(int)Language.English, (int)Language.French}).Build();
            _notMultiSettings = Builder<NewznabSettings>.CreateNew().Build();

            _multiRelease = Builder<ReleaseInfo>.CreateNew().With(r => r.Title = "My German Movie 2017 MULTI")
                .With(r => r.IndexerSettings = _multiSettings).Build();

            _nonMultiRelease = Builder<ReleaseInfo>.CreateNew().With(r => r.Title = "My Movie 2017")
                .With(r => r.IndexerSettings = _notMultiSettings).Build();

            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.All()).Returns(Quality.DefaultQualityDefinitions.ToList());
        }

        public void GivenExtraQD(QualityDefinition definition)
        {
            var defaults = Quality.DefaultQualityDefinitions.ToList();
            defaults.Add(definition);
            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.All()).Returns(defaults);
        }
        
        private void GivenExtraQD(params QualityDefinition[] definition)
        {
            var defaults = Quality.DefaultQualityDefinitions.ToList();
            defaults.AddRange(definition);
            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.All()).Returns(defaults);
        }

        [Test]
        public void should_augment_multi_languages()
        {
            var result = Subject.ParseMovieInfo(_multiRelease.Title, _multiRelease);
            result.Languages.Should().BeEquivalentTo(new List<Language> {Language.English, Language.French});
        }

        [Test]
        public void should_augment_english_no_other_lang_tag_present()
        {
            var result = Subject.ParseMovieInfo(_nonMultiRelease.Title, _nonMultiRelease);
            result.Languages.Should().BeEquivalentTo(new List<Language> {Language.English});
        }

        [TestCase("Movie 2017 Bluray 1080p", "Bluray-1080p")]
        [TestCase("Movie 2017 Bluray Remux 1080p", "Remux-1080p")]
        [TestCase("27.Dresses.2008.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N", "Remux-1080p")]
        [TestCase("The.Nightly.Show.2016.03.14.1080p.WEB.h264-spamTV", "WEBDL-1080p")]
        public void should_correctly_identify_default_definition(string title, string definitionName)
        {
            var result = Subject.ParseMovieInfo(title);
            result.Quality.QualityDefinition.Title.Should().Be(definitionName);
        }

        [Test]
        public void should_correctly_choose_matching_filesize()
        {
            GivenExtraQD(new QualityDefinition
            {
                Title = "Small Bluray 1080p",
                QualityTags = new List<QualityTag>
                {
                    new QualityTag("s_bluray"),
                    new QualityTag("R_1080")
                },
                MaxSize = 50,
                MinSize = 0,
            }, new QualityDefinition
            {
                Title = "Small WEB 1080p",
                QualityTags = new List<QualityTag>
                {
                    new QualityTag("s_webdl"),
                    new QualityTag("R_1080")
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
            var smallRelease = new ReleaseInfo
            {
                Size = 2875.Megabytes() //2.8GB
            };
            var largeRelease = new ReleaseInfo
            {
                Size = 8625.Megabytes() //8.6GB
            };
            var largestRelease = new ReleaseInfo
            {
                Size = 20000.Megabytes() //20GB
            };
            Subject.ParseQualityDefinition(movieInfo, smallRelease).Title.Should().Be("Small Bluray 1080p");
            Subject.ParseQualityDefinition(movieInfo, largeRelease).Title.Should().Be("Bluray-1080p");
            Subject.ParseQualityDefinition(movieInfo, largestRelease).Title.Should().Be("Bluray-1080p");
            Subject.ParseQualityDefinition(webInfo, smallRelease).Title.Should().Be("Small WEB 1080p");
            Subject.ParseQualityDefinition(webInfo, largeRelease).Title.Should().Be("WEBDL-1080p");
            Subject.ParseQualityDefinition(webInfo, largestRelease).Title.Should().Be("WEBDL-1080p");
        }

        [TestCase("Blade.Runner.Directors.Cut.2017.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N",
            "Remux-1080p Director")]
        [TestCase("Blade.Runner.2017.BDREMUX.1080p.Bluray.MULTI.French.English", "Remux-1080p FR")]
        [TestCase("Blade.Runner.2017.BDREMUX.1080p.Bluray.French", "Remux-1080p FR")]
        public void should_correctly_identify_advanced_definitons(string title, string definitionName)
        {
            GivenExtraQD(
                new QualityDefinition
                {
                    Title = "Remux-1080p Director",
                    QualityTags = new List<QualityTag>
                    {
                        new QualityTag("s_bluray"),
                        new QualityTag("R_1080"),
                        new QualityTag("m_remux"),
                        new QualityTag("e_director")
                    }
                },
                new QualityDefinition
                {
                    Title = "Remux-1080p FR",
                    QualityTags = new List<QualityTag>
                    {
                        new QualityTag("s_bluray"),
                        new QualityTag("R_1080"),
                        new QualityTag("m_remux"),
                        new QualityTag("l_re_french"),
                        new QualityTag("l_english")
                    }
                }
            );

            var result = Subject.ParseMovieInfo(title);
            result.Quality.QualityDefinition.Title.Should().Be(definitionName);
        }
    }
}