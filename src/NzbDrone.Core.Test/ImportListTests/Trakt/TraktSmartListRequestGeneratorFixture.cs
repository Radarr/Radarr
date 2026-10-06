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
                SmartListId = "sci-fi-picks-1a2b3c4d5e6f7a8b",
                AccessToken = "token",
                Limit = 250
            };
        }

        [TestCase("https://app.trakt.tv/lists/smart/view/sci-fi-picks-1a2b3c4d5e6f7a8b")]
        [TestCase("not a slug")]
        [TestCase("Sci-Fi-Picks")]
        [TestCase("sci_fi_picks")]
        [TestCase("scí-fi-picks")]
        [TestCase("")]
        public void should_reject_values_that_are_not_smart_list_slugs(string smartListId)
        {
            Subject.Settings.SmartListId = smartListId;

            new TraktSmartListSettingsValidator().Validate(Subject.Settings)
                .Errors.Should().Contain(e => e.PropertyName == nameof(TraktSmartListSettings.SmartListId));
        }

        [TestCase("sci-fi-picks-1a2b3c4d5e6f7a8b")]
        [TestCase("12345")]
        public void should_accept_smart_list_slugs_and_ids(string smartListId)
        {
            Subject.Settings.SmartListId = smartListId;

            new TraktSmartListSettingsValidator().Validate(Subject.Settings)
                .Errors.Should().NotContain(e => e.PropertyName == nameof(TraktSmartListSettings.SmartListId));
        }

        [Test]
        public void should_request_movie_items_of_the_smart_list()
        {
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
