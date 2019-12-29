using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.HDBits;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.HDBitsTests
{
    public class HDBitsRequestGeneratorFixture : CoreTest<HDBitsRequestGenerator>
    {
        private MovieSearchCriteria _movieSearchCriteria;

        [SetUp]
        public void Setup()
        {
            Subject.Settings = new HDBitsSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
                Categories = new[] { 1, 2 },
                ApiKey = "abcd",
                Username = "somename"
            };

            _movieSearchCriteria = new MovieSearchCriteria
            {
                Movie = new Movies.Movie { ImdbId = "tt0076759", Title = "Star Wars", Year = 1977 }
            };
        }

        private void MovieWithoutIMDB()
        {
            _movieSearchCriteria.Movie.ImdbId = null;
        }

        [Test]
        public void should_use_all_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            var encoding = HttpHeader.GetEncodingFromContentType(page.HttpRequest.Headers.ContentType);

            var body = encoding.GetString(page.HttpRequest.ContentData);
            var query = JsonConvert.DeserializeObject<TorrentQuery>(body);

            query.Category.Should().HaveCount(2);
            query.Username.Should().Be(Subject.Settings.Username);
            query.Passkey.Should().Be(Subject.Settings.ApiKey);
        }

        [Test]
        public void should_search_by_imdbid_if_supported()
        {
            var results = Subject.GetSearchRequests(_movieSearchCriteria);
            var imdbQuery = int.Parse(_movieSearchCriteria.Movie.ImdbId.Substring(2));

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            var encoding = HttpHeader.GetEncodingFromContentType(page.HttpRequest.Headers.ContentType);

            var body = encoding.GetString(page.HttpRequest.ContentData);
            var query = JsonConvert.DeserializeObject<TorrentQuery>(body);

            query.Category.Should().HaveCount(2);
            query.ImdbInfo.Id.Should().Be(imdbQuery);
        }

        [Test]
        public void should_return_no_results_if_no_imdb()
        {
            MovieWithoutIMDB();

            var results = Subject.GetSearchRequests(_movieSearchCriteria);
            results.GetTier(0).Should().HaveCount(0);
        }
    }
}
