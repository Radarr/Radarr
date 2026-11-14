using System;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class GetJsonVersionFixture : CoreTest<XbmcJsonApiProxy>
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

        [TestCase("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":{\"major\":13,\"minor\":5,\"patch\":0}}}", 13, 5, 0)]
        [TestCase("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":{\"major\":6,\"minor\":32,\"patch\":5}}}", 6, 32, 5)]
        public void should_read_json_rpc_version(string response, int major, int minor, int patch)
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => _request = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), response));

            Subject.GetJsonVersion(_settings).Should().Be(new Version(major, minor, patch));

            Encoding.UTF8.GetString(_request.ContentData).Should().Contain("\"JSONRPC.Version\"");
        }
    }
}
