using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.Trakt.SmartList;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Trakt
{
    public class TraktSmartListRequestGeneratorFixture : CoreTest<TraktSmartListRequestGenerator>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ITraktProxy>()
                .Setup(s => s.BuildRequest(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>()))
                .Returns<string, HttpMethod, string>((resource, method, token) =>
                    new HttpRequestBuilder("https://api.trakt.tv").Resource(resource).Build());

            Subject.Settings = new TraktSmartListSettings
            {
                SmartList = "sci-fi-picks-1a2b3c4d5e6f7a8b",
                AccessToken = "token",
                Limit = 250
            };
        }

        [TestCase("sci-fi-picks-1a2b3c4d5e6f7a8b")]
        [TestCase("  sci-fi-picks-1a2b3c4d5e6f7a8b/ ")]
        [TestCase("https://app.trakt.tv/lists/smart/view/sci-fi-picks-1a2b3c4d5e6f7a8b")]
        [TestCase("https://app.trakt.tv/lists/smart/view/sci-fi-picks-1a2b3c4d5e6f7a8b?sort=added#top")]
        [TestCase("https://api.trakt.tv/smart-lists/sci-fi-picks-1a2b3c4d5e6f7a8b/items")]
        public void should_parse_slug_from_url_or_slug(string smartList)
        {
            TraktSmartListSlug.Parse(smartList).Should().Be("sci-fi-picks-1a2b3c4d5e6f7a8b");
        }

        [TestCase("https://trakt.tv/users/someone/lists/regular-list")]
        [TestCase("https://app.trakt.tv/users/someone/lists/regular-list")]
        [TestCase("not a slug")]
        [TestCase("")]
        public void should_reject_values_that_are_not_smart_lists(string smartList)
        {
            TraktSmartListSlug.IsValid(smartList).Should().BeFalse();
        }

        [TestCase("sci-fi-picks-1a2b3c4d5e6f7a8b")]
        [TestCase("https://app.trakt.tv/lists/smart/view/sci-fi-picks-1a2b3c4d5e6f7a8b")]
        public void should_accept_smart_list_urls_and_slugs(string smartList)
        {
            TraktSmartListSlug.IsValid(smartList).Should().BeTrue();
        }

        [Test]
        public void should_request_movie_items_of_the_smart_list()
        {
            Subject.Settings.SmartList = "https://app.trakt.tv/lists/smart/view/sci-fi-picks-1a2b3c4d5e6f7a8b";

            var tiers = Subject.GetMovies().GetAllTiers().ToList();

            tiers.Should().HaveCount(1);

            var request = tiers.First().First().HttpRequest;

            request.Url.FullUri.Should().Be("https://api.trakt.tv/smart-lists/sci-fi-picks-1a2b3c4d5e6f7a8b/items/movies?limit=250");
        }

        [Test]
        public void should_send_the_access_token()
        {
            Subject.GetMovies().GetAllTiers().First().ToList();

            Mocker.GetMock<ITraktProxy>()
                .Verify(v => v.BuildRequest(It.IsAny<string>(), HttpMethod.Get, "token"), Times.Once());
        }
    }
}
