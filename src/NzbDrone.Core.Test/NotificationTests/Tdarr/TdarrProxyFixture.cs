using System.Text;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Tdarr;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Tdarr
{
    [TestFixture]
    public class TdarrProxyFixture : CoreTest<TdarrProxy>
    {
        private TdarrSettings _settings;
        private HttpRequest _request;

        [SetUp]
        public void SetUp()
        {
            _settings = new TdarrSettings
            {
                Host = "tdarr.local",
                Port = 8265,
                LibraryId = "library-id",
                ApiKey = "tapi_test_key"
            };

            Mocker.GetMock<IHttpClient>()
                  .Setup(v => v.Execute(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(request => _request = request)
                  .Returns<HttpRequest>(request => new HttpResponse(request, new HttpHeader(), "OK"));
        }

        [Test]
        public void should_send_scan_request_for_file()
        {
            Subject.ScanFile("/movies/Test Movie (2026)/movie.mkv", _settings);

            _request.Method.Should().Be(System.Net.Http.HttpMethod.Post);
            _request.Url.FullUri.Should().Be("http://tdarr.local:8265/api/v2/scan-files");
            _request.Headers.GetSingleValue("x-api-key").Should().Be("tapi_test_key");

            var payload = JObject.Parse(Encoding.UTF8.GetString(_request.ContentData));
            payload["data"]?["scanConfig"]?["dbID"]?.Value<string>().Should().Be("library-id");
            payload["data"]?["scanConfig"]?["mode"]?.Value<string>().Should().Be("scanFolderWatcher");
            payload["data"]?["scanConfig"]?["arrayOrPath"]?[0]?.Value<string>().Should().Be("/movies/Test Movie (2026)/movie.mkv");
        }

        [Test]
        public void should_test_status_endpoint()
        {
            Subject.Test(_settings);

            _request.Method.Should().Be(System.Net.Http.HttpMethod.Get);
            _request.Url.FullUri.Should().Be("http://tdarr.local:8265/api/v2/status");
        }
    }
}
