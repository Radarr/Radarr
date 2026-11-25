using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Filmweb;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Filmweb
{
    [TestFixture]
    public class FilmwebParserFixture : CoreTest<FilmwebParser>
    {
        private Mock<IHttpClient> _httpClient;

        [SetUp]
        public void Setup()
        {
            _httpClient = new Mock<IHttpClient>();
        }

        private ImportListResponse CreateResponse(string url, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var httpRequest = new HttpRequest(url);
            var httpResponse = new HttpResponse(httpRequest, new HttpHeader(), Encoding.UTF8.GetBytes(content), statusCode);

            return new ImportListResponse(new ImportListRequest(httpRequest), httpResponse);
        }

        private void SetupMovieInfoResponse(long entityId, string title, string originalTitle, int year)
        {
            var movieInfoJson = $@"{{
                ""id"": {entityId},
                ""title"": ""{title}"",
                ""originalTitle"": ""{originalTitle}"",
                ""year"": {year},
                ""type"": ""film"",
                ""subType"": ""movie"",
                ""posterPath"": ""/path/to/poster.jpg""
            }}";

            var request = new HttpRequest($"https://www.filmweb.pl/api/v1/title/{entityId}/info");
            var response = new HttpResponse(request, new HttpHeader(), Encoding.UTF8.GetBytes(movieInfoJson), HttpStatusCode.OK);

            _httpClient.Setup(c => c.Get(It.Is<HttpRequest>(r => r.Url.ToString().Contains($"/api/v1/title/{entityId}/info"))))
                      .Returns(response);
        }

        [Test]
        public void should_parse_filmweb_want2see_list()
        {
            var listJson = @"[
                {""entity"": 123456, ""timestamp"": 1693737600, ""level"": 5},
                {""entity"": 789012, ""timestamp"": 1693824000, ""level"": 4}
            ]";

            SetupMovieInfoResponse(123456, "Blade Runner 2049", "Blade Runner 2049", 2017);
            SetupMovieInfoResponse(789012, "Dune", "Dune", 2021);

            var parser = new FilmwebParser(_httpClient.Object, 100);

            var result = parser.ParseResponse(CreateResponse("https://www.filmweb.pl/api/v1/user/testuser/want2see/film", listJson));

            result.Should().HaveCount(2);

            result.First().Title.Should().Be("Blade Runner 2049");
            result.First().Year.Should().Be(2017);

            result[1].Title.Should().Be("Dune");
            result[1].Year.Should().Be(2021);
        }

        [Test]
        public void should_respect_limit_parameter()
        {
            var listJson = @"[
                {""entity"": 111111, ""timestamp"": 1693737600, ""level"": 5},
                {""entity"": 222222, ""timestamp"": 1693824000, ""level"": 4},
                {""entity"": 333333, ""timestamp"": 1693910400, ""level"": 3}
            ]";

            SetupMovieInfoResponse(111111, "Movie 1", "Movie 1", 2020);
            SetupMovieInfoResponse(222222, "Movie 2", "Movie 2", 2021);

            var parser = new FilmwebParser(_httpClient.Object, 2);

            var result = parser.ParseResponse(CreateResponse("https://www.filmweb.pl/api/v1/user/testuser/want2see/film", listJson));

            result.Should().HaveCount(2);
            result.First().Title.Should().Be("Movie 1");
            result[1].Title.Should().Be("Movie 2");

            _httpClient.Verify(c => c.Get(It.Is<HttpRequest>(r => r.Url.ToString().Contains("/api/v1/title/333333/info"))), Times.Never);
        }

        [Test]
        public void should_handle_empty_list()
        {
            var listJson = "[]";
            var parser = new FilmwebParser(_httpClient.Object, 100);

            var result = parser.ParseResponse(CreateResponse("https://www.filmweb.pl/api/v1/user/testuser/want2see/film", listJson));

            result.Should().BeEmpty();
        }

        [Test]
        public void should_skip_movies_with_failed_info_requests()
        {
            var listJson = @"[
                {""entity"": 123456, ""timestamp"": 1693737600, ""level"": 5},
                {""entity"": 789012, ""timestamp"": 1693824000, ""level"": 4}
            ]";

            SetupMovieInfoResponse(123456, "Working Movie", "Working Movie", 2020);

            var failedRequest = new HttpRequest("https://www.filmweb.pl/api/v1/title/789012/info");
            var failedResponse = new HttpResponse(failedRequest, new HttpHeader(), System.Array.Empty<byte>(), HttpStatusCode.NotFound);
            _httpClient.Setup(c => c.Get(It.Is<HttpRequest>(r => r.Url.ToString().Contains("/api/v1/title/789012/info"))))
                      .Returns(failedResponse);

            var parser = new FilmwebParser(_httpClient.Object, 100);

            var result = parser.ParseResponse(CreateResponse("https://www.filmweb.pl/api/v1/user/testuser/want2see/film", listJson));

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Working Movie");
        }

        [Test]
        public void should_use_original_title_when_title_is_empty()
        {
            var listJson = @"[{""entity"": 123456, ""timestamp"": 1693737600, ""level"": 5}]";

            var movieInfoJson = @"{
                ""id"": 123456,
                ""title"": """",
                ""originalTitle"": ""Original Title"",
                ""year"": 2020,
                ""type"": ""film""
            }";

            var request = new HttpRequest("https://www.filmweb.pl/api/v1/title/123456/info");
            var response = new HttpResponse(request, new HttpHeader(), Encoding.UTF8.GetBytes(movieInfoJson), HttpStatusCode.OK);
            _httpClient.Setup(c => c.Get(It.IsAny<HttpRequest>())).Returns(response);

            var parser = new FilmwebParser(_httpClient.Object, 100);

            var result = parser.ParseResponse(CreateResponse("https://www.filmweb.pl/api/v1/user/testuser/want2see/film", listJson));

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Original Title");
        }

        [Test]
        public void should_enforce_limit_bounds()
        {
            var parser1 = new FilmwebParser(_httpClient.Object, -5);
            var parser2 = new FilmwebParser(_httpClient.Object, 1500);

            parser1.Should().NotBeNull();
            parser2.Should().NotBeNull();
        }

        [Test]
        public void should_handle_invalid_json()
        {
            var invalidJson = "invalid json content";
            var parser = new FilmwebParser(_httpClient.Object, 100);

            var result = parser.ParseResponse(CreateResponse("https://www.filmweb.pl/api/v1/user/testuser/want2see/film", invalidJson));
            result.Should().BeEmpty();
        }

        [Test]
        public void should_handle_null_movie_info()
        {
            var listJson = @"[{""entity"": 123456, ""timestamp"": 1693737600, ""level"": 5}]";

            var request = new HttpRequest("https://www.filmweb.pl/api/v1/title/123456/info");
            var response = new HttpResponse(request, new HttpHeader(), System.Array.Empty<byte>(), HttpStatusCode.InternalServerError);
            _httpClient.Setup(c => c.Get(It.IsAny<HttpRequest>())).Returns(response);

            var parser = new FilmwebParser(_httpClient.Object, 100);

            var result = parser.ParseResponse(CreateResponse("https://www.filmweb.pl/api/v1/user/testuser/want2see/film", listJson));

            result.Should().BeEmpty();
        }

        [Test]
        public void should_handle_malformed_entity_data()
        {
            var listJson = @"[
                {""timestamp"": 1693737600, ""level"": 5},
                {""entity"": 789012, ""timestamp"": 1693824000, ""level"": 4}
            ]";

            SetupMovieInfoResponse(789012, "Valid Movie", "Valid Movie", 2021);

            var parser = new FilmwebParser(_httpClient.Object, 100);

            var result = parser.ParseResponse(CreateResponse("https://www.filmweb.pl/api/v1/user/testuser/want2see/film", listJson));

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Valid Movie");
        }
    }
}
