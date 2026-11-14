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
    public class GetMoviesFixture : CoreTest<XbmcJsonApiProxy>
    {
        private const string KODI_21_RESPONSE = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":1,\"start\":0,\"total\":1},\"movies\":[{\"file\":\"/movies/Movie (2025)/Movie.mkv\",\"imdbnumber\":\"803796\",\"label\":\"Movie\",\"movieid\":13,\"uniqueid\":{\"imdb\":\"tt14205554\",\"tmdb\":\"803796\"}}]}}";

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

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => _request = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), KODI_21_RESPONSE));
        }

        [Test]
        public void should_request_unique_ids()
        {
            Subject.GetMovies(_settings, true);

            Encoding.UTF8.GetString(_request.ContentData).Should().Contain("\"uniqueid\"");
        }

        [Test]
        public void should_not_request_unique_ids_when_not_included()
        {
            Subject.GetMovies(_settings, false);

            var content = Encoding.UTF8.GetString(_request.ContentData);

            content.Should().Contain("\"imdbnumber\"");
            content.Should().NotContain("\"uniqueid\"");
        }

        [Test]
        public void should_read_unique_ids()
        {
            var movie = Subject.GetMovies(_settings, true).Should().ContainSingle().Subject;

            movie.ImdbNumber.Should().Be("803796");
            movie.UniqueId.Should().Contain("imdb", "tt14205554").And.Contain("tmdb", "803796");
        }
    }
}
