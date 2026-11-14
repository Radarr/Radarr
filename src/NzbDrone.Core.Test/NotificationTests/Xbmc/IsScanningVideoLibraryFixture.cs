using System.Text;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class IsScanningVideoLibraryFixture : CoreTest<XbmcJsonApiProxy>
    {
        private XbmcSettings _settings;
        private HttpRequest _request;

        [SetUp]
        public void Setup()
        {
            _settings = new XbmcSettings
            {
                Host = "localhost",
                Port = 8080
            };
        }

        private void GivenResponse(string content)
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => _request = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), content));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void should_read_scanning_state(bool isScanning)
        {
            GivenResponse("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"Library.IsScanningVideo\":" + isScanning.ToString().ToLowerInvariant() + "}}");

            Subject.IsScanningVideoLibrary(_settings).Should().Be(isScanning);
        }

        [Test]
        public void should_not_be_scanning_when_kodi_does_not_return_the_scanning_state()
        {
            GivenResponse("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{}}");

            Subject.IsScanningVideoLibrary(_settings).Should().BeFalse();
        }

        [Test]
        public void should_request_the_scanning_state()
        {
            GivenResponse("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"Library.IsScanningVideo\":false}}");

            Subject.IsScanningVideoLibrary(_settings);

            var request = JObject.Parse(Encoding.UTF8.GetString(_request.ContentData));

            request["method"].ToString().Should().Be("XBMC.GetInfoBooleans");
            request["params"].ToString(Formatting.None).Should().Be("[[\"Library.IsScanningVideo\"]]");
        }
    }
}
