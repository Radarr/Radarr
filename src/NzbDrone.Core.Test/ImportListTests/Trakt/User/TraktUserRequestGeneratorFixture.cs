using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.Trakt.User;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.Test.ImportListTests.Trakt.User
{
    public class TraktUserRequestGeneratorFixture
    {
        private Mock<ITraktProxy> _traktProxy;

        [SetUp]
        public void Setup()
        {
            _traktProxy = new Mock<ITraktProxy>();
            _traktProxy.Setup(p => p.BuildRequest(It.IsAny<HttpRequest>(), It.IsAny<string>()))
                .Returns((HttpRequest request, string accessToken) => request);
        }

        private static TraktUserSettings CreateDefaultSettings()
        {
            return new TraktUserSettings
            {
                Username = "testuser",
                AccessToken = "token",
            };
        }

        [Test]
        public void should_request_single_page_when_limit_is_250_or_less()
        {
            var settings = CreateDefaultSettings();
            settings.Limit = 100;

            var generator = new TraktUserRequestGenerator(_traktProxy.Object, settings);

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

            var generator = new TraktUserRequestGenerator(_traktProxy.Object, settings);

            var requests = generator.GetMovies().GetAllTiers().First().ToList();

            requests.Should().HaveCount(2);
            requests[0].Url.FullUri.Should().Contain("page=1");
            requests[0].Url.FullUri.Should().Contain("limit=250");
            requests[1].Url.FullUri.Should().Contain("page=2");
            requests[1].Url.FullUri.Should().Contain("limit=50");
        }
    }
}
