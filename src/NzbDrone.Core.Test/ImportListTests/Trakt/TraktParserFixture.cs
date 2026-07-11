using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Trakt;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Trakt
{
    public class TraktParserFixture : CoreTest<TraktParser>
    {
        private ImportListResponse CreateResponse(string url, string content)
        {
            var httpRequest = new HttpRequest(url);
            var httpResponse = new HttpResponse(httpRequest, new HttpHeader(), Encoding.UTF8.GetBytes(content));

            return new ImportListResponse(new ImportListRequest(httpRequest), httpResponse);
        }

        [Test]
        public void should_parse_movie_with_valid_tmdb_id()
        {
            var json = @"[
              {
                ""type"": ""movie"",
                ""movie"": {
                  ""title"": ""Rogue One: A Star Wars Story"",
                  ""year"": 2016,
                  ""ids"": { ""trakt"": 190420, ""slug"": ""rogue-one"", ""imdb"": ""tt3748528"", ""tmdb"": 330459 }
                }
              }
            ]";

            var result = Subject.ParseResponse(CreateResponse("http://api.trakt.tv/users/me/watchlist/movies", json));

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Rogue One: A Star Wars Story");
            result.First().TmdbId.Should().Be(330459);
            result.First().ImdbId.Should().Be("tt3748528");
            result.First().Year.Should().Be(2016);
        }

        [Test]
        public void should_not_throw_when_entry_has_null_tmdb_id()
        {
            var json = @"[
              {
                ""type"": ""movie"",
                ""movie"": {
                  ""title"": ""Nosferatu"",
                  ""year"": null,
                  ""ids"": { ""trakt"": 287264, ""slug"": ""nosferatu"", ""imdb"": null, ""tmdb"": null }
                }
              }
            ]";

            var result = Subject.ParseResponse(CreateResponse("http://api.trakt.tv/users/me/watchlist/movies", json));

            result.Should().HaveCount(1);
            result.First().TmdbId.Should().Be(0);
        }
    }
}
