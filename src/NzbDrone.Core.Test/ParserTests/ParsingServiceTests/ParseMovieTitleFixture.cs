using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
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
    }
}