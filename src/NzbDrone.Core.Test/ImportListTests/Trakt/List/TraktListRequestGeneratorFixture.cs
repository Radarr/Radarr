using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.Trakt.List;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.Test.ImportListTests.Trakt.List
{
    public class TraktListRequestGeneratorFixture
    {
        private Mock<ITraktProxy> _traktProxy;

        [SetUp]
        public void Setup()
        {
            _traktProxy = new Mock<ITraktProxy>();
            _traktProxy.Setup(p => p.BuildRequest(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>()))
                .Returns((string resource, HttpMethod method, string accessToken) => new HttpRequest($"https://api.trakt.tv/{resource}"));
        }

        private static TraktListSettings CreateDefaultSettings()
        {
            return new TraktListSettings
            {
                Username = "testuser",
                Listname = "my list",
                AccessToken = "token",
            };
        }

        [Test]
        public void should_request_single_page_when_limit_is_250_or_less()
        {
            var settings = CreateDefaultSettings();
            settings.Limit = 100;

            var generator = new TraktListRequestGenerator(_traktProxy.Object)
            {
                Settings = settings
            };

            var requests = generator.GetMovies().GetAllTiers().First().ToList();

            requests.Should().HaveCount(1);
            requests[0].Url.FullUri.Should().Contain("limit=100");
            requests[0].Url.FullUri.Should().Contain("page=1");
        }

        [Test]
        public void should_request_multiple_pages_when_limit_exceeds_250()
        {
            var settings = CreateDefaultSettings();
            settings.Limit = 300;

            var generator = new TraktListRequestGenerator(_traktProxy.Object) { Settings = settings };

            var requests = generator.GetMovies().GetAllTiers().First().ToList();

            requests.Should().HaveCount(2);
            requests[0].Url.FullUri.Should().Contain("page=1");
            requests[0].Url.FullUri.Should().Contain("limit=250");
            requests[1].Url.FullUri.Should().Contain("page=2");
            requests[1].Url.FullUri.Should().Contain("limit=50");
        }
    }
}
