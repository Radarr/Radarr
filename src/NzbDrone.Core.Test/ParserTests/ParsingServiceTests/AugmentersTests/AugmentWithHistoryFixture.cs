using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Rarbg;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.AugmentersTests
{
    [TestFixture]
    public class AugmentWithHistoryFixture : AugmentMovieInfoFixture<AugmentWithHistory>
    {
        private AugmentWithHistory _customSubject { get; set; }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            //Add multi indexer
            GivenIndexerSettings(new RarbgSettings
            {
                MultiLanguages = new List<int>
                {
                    (int)Language.English,
                    (int)Language.French,
                }
            });
        }

        protected new AugmentWithHistory Subject
        {
            get
            {
                if (_customSubject == null)
                {
                    _customSubject = new AugmentWithHistory(Mocker.GetMock<IIndexerFactory>().Object, new List<IAugmentParsedMovieInfo> { Mocker.Resolve<AugmentWithReleaseInfo>() });
                }

                return _customSubject;
            }
        }

        private void GivenIndexerSettings(IIndexerSettings indexerSettings)
        {
            Mocker.GetMock<IIndexerFactory>().Setup(f => f.Get(It.IsAny<int>())).Returns(new IndexerDefinition
            {
                Settings = indexerSettings
            });
        }

        private History.History HistoryWithData(params string[] data)
        {
            var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            for (var i = 0; i < data.Length; i += 2)
            {
                dict.Add(data[i], data[i + 1]);
            }

            return new History.History
            {
                Data = dict,
                EventType = HistoryEventType.Grabbed
            };
        }

        [Test]
        public void should_add_indexer_flags()
        {
            var history = HistoryWithData("IndexerFlags", (IndexerFlags.PTP_Approved | IndexerFlags.PTP_Golden).ToString());
            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, history);
            movieInfo.ExtraInfo["IndexerFlags"].Should().BeEquivalentTo(IndexerFlags.PTP_Golden | IndexerFlags.PTP_Approved);
        }

        [Test]
        public void should_add_size()
        {
            var history = HistoryWithData("Size", 1500.ToString());
            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, history);
            movieInfo.ExtraInfo["Size"].Should().BeEquivalentTo(1500);
        }

        [Test]
        public void should_use_settings_languages_when_necessary()
        {
            var history = HistoryWithData("IndexerId", 1.ToString());

            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, history);
            movieInfo.Languages.Should().BeEquivalentTo();

            MovieInfo.SimpleReleaseTitle = "A Movie 1998 Bluray 1080p MULTI";
            var multiInfo = Subject.AugmentMovieInfo(MovieInfo, history);
            multiInfo.Languages.Should().BeEquivalentTo(Language.English, Language.French);
        }

        [Test]
        public void should_not_use_settings_languages()
        {
            var unknownIndexer = HistoryWithData();
            var unknownIndexerInfo = Subject.AugmentMovieInfo(MovieInfo, unknownIndexer);
            unknownIndexerInfo.Languages.Should().BeEquivalentTo();
        }
    }
}
