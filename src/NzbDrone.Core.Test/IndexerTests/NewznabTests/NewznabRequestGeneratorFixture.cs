using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    public class NewznabRequestGeneratorFixture : CoreTest<NewznabRequestGenerator>
    {
        private MovieSearchCriteria _movieSearchCriteria;
        private NewznabCapabilities _capabilities;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new NewznabSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
                Categories = new[] { 1, 2 },
                ApiKey = "abcd",
            };

            _movieSearchCriteria = new MovieSearchCriteria
            {
                Movie = new Movies.Movie { ImdbId = "tt0076759", Title = "Star Wars", Year = 1977, TmdbId = 11 },
                SceneTitles = new List<string> { "Star Wars" }
            };

            _capabilities = new NewznabCapabilities();

            Mocker.GetMock<INewznabCapabilitiesProvider>()
                .Setup(v => v.GetCapabilities(It.IsAny<NewznabSettings>()))
                .Returns(_capabilities);
        }

        [Test]
        public void should_use_all_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("&cat=1,2&");
        }

        [Test]
        public void should_not_have_duplicate_categories()
        {
            Subject.Settings.Categories = new[] { 1, 2, 2, 3 };

            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("&cat=1,2,3&");
        }

        [Test]
        public void should_return_subsequent_pages()
        {
            var results = Subject.GetSearchRequests(_movieSearchCriteria);

            results.GetAllTiers().Should().HaveCount(2);

            var pages = results.GetAllTiers().First().Take(3).ToList();

            pages[0].Url.FullUri.Should().Contain("&offset=0&");
            pages[1].Url.FullUri.Should().Contain("&offset=100&");
            pages[2].Url.FullUri.Should().Contain("&offset=200&");
        }

        [Test]
        public void should_not_get_unlimited_pages()
        {
            var results = Subject.GetSearchRequests(_movieSearchCriteria);

            results.GetAllTiers().Should().HaveCount(2);

            var pages = results.GetAllTiers().First().Take(500).ToList();

            pages.Count.Should().BeLessThan(500);
        }

        [Test]
        public void should_not_search_by_imdbid_if_not_supported()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q" };

            var results = Subject.GetSearchRequests(_movieSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().NotContain("imdbid=0076759");
            page.Url.Query.Should().Contain("q=Star");
        }

        [Test]
        public void should_search_by_imdbid_if_supported()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q", "imdbid" };

            var results = Subject.GetSearchRequests(_movieSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("imdbid=0076759");
        }

        [Test]
        public void should_search_by_tmdbid_if_supported()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q", "tmdbid" };

            var results = Subject.GetSearchRequests(_movieSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("tmdbid=11");
        }

        [Test]
        public void should_prefer_search_by_tmdbid_if_rid_supported()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q", "tmdbid", "imdbid" };

            var results = Subject.GetSearchRequests(_movieSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("tmdbid=11");
            page.Url.Query.Should().NotContain("imdbid=0076759");
        }

        [Test]
        public void should_use_aggregrated_id_search_if_supported()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q", "tmdbid", "imdbid" };
            _capabilities.SupportsAggregateIdSearch = true;

            var results = Subject.GetSearchRequests(_movieSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("tmdbid=11");
            page.Url.Query.Should().Contain("imdbid=0076759");
        }

        [Test]
        public void should_not_use_aggregrated_id_search_if_no_ids_supported()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q" };
            _capabilities.SupportsAggregateIdSearch = true; // Turns true if indexer supplies supportedParams.

            var results = Subject.GetSearchRequests(_movieSearchCriteria);
            results.Tiers.Should().Be(1);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_not_use_aggregrated_id_search_if_no_ids_are_known()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q", "imdbid" };
            _capabilities.SupportsAggregateIdSearch = true; // Turns true if indexer supplies supportedParams.

            _movieSearchCriteria.Movie.ImdbId = null;

            var results = Subject.GetSearchRequests(_movieSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_fallback_to_q()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q", "tmdbid", "imdbid" };
            _capabilities.SupportsAggregateIdSearch = true;

            var results = Subject.GetSearchRequests(_movieSearchCriteria);
            results.Tiers.Should().Be(2);

            var pageTier2 = results.GetTier(1).First().First();

            pageTier2.Url.Query.Should().NotContain("tmdbid=11");
            pageTier2.Url.Query.Should().NotContain("imdbid=0076759");
            pageTier2.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_encode_raw_title()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q" };
            _capabilities.TextSearchEngine = "raw";

            MovieSearchCriteria movieRawSearchCriteria = new MovieSearchCriteria
            {
                Movie = new Movies.Movie { Title = "Some Movie & Title: Words", Year = 2021, TmdbId = 123 },
                SceneTitles = new List<string> { "Some Movie & Title: Words" }
            };

            var results = Subject.GetSearchRequests(movieRawSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=Some%20Movie%20%26%20Title%3A%20Words");
            page.Url.Query.Should().NotContain(" & ");
            page.Url.Query.Should().Contain("%26");
        }

        [Test]
        public void should_use_clean_title_and_encode()
        {
            _capabilities.SupportedMovieSearchParameters = new[] { "q" };
            _capabilities.TextSearchEngine = "sphinx";

            MovieSearchCriteria movieRawSearchCriteria = new MovieSearchCriteria
            {
                Movie = new Movies.Movie { Title = "Some Movie & Title: Words", Year = 2021, TmdbId = 123 },
                SceneTitles = new List<string> { "Some Movie & Title: Words" }
            };

            var results = Subject.GetSearchRequests(movieRawSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=Some%20Movie%20and%20Title%20Words%202021");
            page.Url.Query.Should().Contain("and");
            page.Url.Query.Should().NotContain(" & ");
            page.Url.Query.Should().NotContain("%26");
        }
    }
}
