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
        
        [TestCase("Blade.Runner.Directors.Cut.2017.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N", "Remux-1080p Director")]
        public void should_correctly_identify_advanced_definitons(string title, string definitionName)
        {
            GivenExtraQD(new QualityDefinition { Title = "Remux-1080p Director", QualityTags = new List<QualityTag>{ new QualityTag("s_bluray"), new QualityTag("R_1080"), new QualityTag("m_remux"), new QualityTag("e_director") }});
            var result = Subject.ParseMovieInfo(title);
            result.Quality.QualityDefinition.Title.Should().Be(definitionName);
        }
    }
}