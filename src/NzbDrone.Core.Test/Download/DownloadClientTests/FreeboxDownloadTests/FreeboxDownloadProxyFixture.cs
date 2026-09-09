using System;
using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.FreeboxDownload;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.FreeboxDownloadTests
{
    [TestFixture]
    public class FreeboxDownloadProxyFixture : CoreTest<FreeboxDownloadProxy>
    {
        protected FreeboxDownloadSettings _settings;
        protected string _cachedHeaderKeyValue;
        protected byte[] _successfulEmptyResponseBytes;

        protected Mock<ICached<string>> _cachedHeaderKey;

        [SetUp]
        public void Setup()
        {
            _settings = new FreeboxDownloadSettings()
            {
                Host = "127.0.0.1",
                Port = 443,
                ApiUrl = "/api/v1/",
                AppId = "someid",
                AppToken = "S0mEv3RY1oN9T0k3n"
            };

            _cachedHeaderKeyValue = "abcdefg123456";

            var rawSuccessfulEmptyResponse = "{ \"success\": true }";
            _successfulEmptyResponseBytes = Encoding.UTF8.GetBytes(rawSuccessfulEmptyResponse);

            _cachedHeaderKey = Mocker.GetMock<ICached<string>>();
            Mocker.GetMock<ICacheManager>()
                .Setup(c => c.GetCache<string>(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns(_cachedHeaderKey.Object);
        }

        protected void GivenCachedHeaderKey()
        {
            _cachedHeaderKey.Setup(c => c.Find(It.IsAny<string>()))
              .Returns<string>(r => _cachedHeaderKeyValue);
        }

        protected void GivenSuccessfulEmptyResponse()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), _successfulEmptyResponseBytes, statusCode: HttpStatusCode.OK));
        }

        [Test]
        public void GetTasks_WithSuccessfulEmptyResponse_ShouldBeEmpty()
        {
            GivenCachedHeaderKey();
            GivenSuccessfulEmptyResponse();

            Subject.GetTasks(_settings).Should().BeEmpty();
        }
    }
}
