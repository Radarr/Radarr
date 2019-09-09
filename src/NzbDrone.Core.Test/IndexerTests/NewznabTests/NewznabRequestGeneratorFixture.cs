using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    public class NewznabRequestGeneratorFixture : CoreTest<NewznabRequestGenerator>
    {
        private AlbumSearchCriteria _singleAlbumSearchCriteria;
        private NewznabCapabilities _capabilities;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new NewznabSettings()
            {
                 BaseUrl = "http://127.0.0.1:1234/",
                 Categories = new [] { 1, 2 },
                 ApiKey = "abcd",
            };

            _singleAlbumSearchCriteria = new AlbumSearchCriteria
            {
                Artist = new Music.Artist { Name = "Alien Ant Farm" },
                AlbumTitle = "TruANT"

            };

            _capabilities = new NewznabCapabilities();

            Mocker.GetMock<INewznabCapabilitiesProvider>()
                .Setup(v => v.GetCapabilities(It.IsAny<NewznabSettings>()))
                .Returns(_capabilities);
        }

        [Test]
        public void should_use_all_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("&cat=1,2&");
        }

        [Test]
        public void should_search_by_artist_and_album_if_supported()
        {
            _capabilities.SupportedAudioSearchParameters = new[] { "q", "artist", "album"};

            var results = Subject.GetSearchRequests(_singleAlbumSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("artist=Alien%20Ant%20Farm");
            page.Url.Query.Should().Contain("album=TruANT");
        }
    }
}
