using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
        public class ReleaseSearchServiceFixture : CoreTest<ReleaseSearchService>
    {
        private Mock<IIndexer> _mockIndexer;
        private Movie _movie;

        [SetUp]
        public void SetUp()
        {
            _mockIndexer = Mocker.GetMock<IIndexer>();
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition { Id = 1 });
            _mockIndexer.SetupGet(s => s.SupportsSearch).Returns(true);

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.AutomaticSearchEnabled(true))
                  .Returns(new List<IIndexer> { _mockIndexer.Object });

            Mocker.GetMock<IMakeDownloadDecision>()
                .Setup(s => s.GetSearchDecision(It.IsAny<List<Parser.Model.ReleaseInfo>>(), It.IsAny<SearchCriteriaBase>()))
                .Returns(new List<DownloadDecision>());

            _movie = Builder<Movie>.CreateNew()
                .With(v => v.Monitored = true)
                .Build();

            Mocker.GetMock<IMovieService>()
                .Setup(v => v.GetMovie(_movie.Id))
                .Returns(_movie);

            Mocker.GetMock<IMovieTranslationService>()
                .Setup(s => s.GetAllTranslationsForMovieMetadata(It.IsAny<int>()))
                .Returns(new List<MovieTranslation>());
        }

        private List<SearchCriteriaBase> WatchForSearchCriteria()
        {
            var result = new List<SearchCriteriaBase>();

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<MovieSearchCriteria>()))
                .Callback<MovieSearchCriteria>(s => result.Add(s))
                .Returns(new List<Parser.Model.ReleaseInfo>());

            return result;
        }

        [Test]
        public void Tags_IndexerTags_MovieNoTags_IndexerNotIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 3 }
            });

            var allCriteria = WatchForSearchCriteria();

            Subject.MovieSearch(_movie, true, false);

            var criteria = allCriteria.OfType<MovieSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public void Tags_IndexerNoTags_MovieTags_IndexerIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1
            });

            _movie = Builder<Movie>.CreateNew()
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 3 })
                .Build();

            Mocker.GetMock<IMovieService>()
                .Setup(v => v.GetMovie(_movie.Id))
                .Returns(_movie);

            var allCriteria = WatchForSearchCriteria();

            Subject.MovieSearch(_movie, true, false);

            var criteria = allCriteria.OfType<MovieSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
        }

        [Test]
        public void Tags_IndexerAndMovieTagsMatch_IndexerIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 1, 2, 3 }
            });

            _movie = Builder<Movie>.CreateNew()
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 3, 4, 5 })
                .Build();

            Mocker.GetMock<IMovieService>()
                .Setup(v => v.GetMovie(_movie.Id))
                .Returns(_movie);

            var allCriteria = WatchForSearchCriteria();

            Subject.MovieSearch(_movie, true, false);

            var criteria = allCriteria.OfType<MovieSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
        }

        [Test]
        public void Tags_IndexerAndMovieTagsMismatch_IndexerNotIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 1, 2, 3 }
            });

            _movie = Builder<Movie>.CreateNew()
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 4, 5, 6 })
                .Build();

            Mocker.GetMock<IMovieService>()
                .Setup(v => v.GetMovie(_movie.Id))
                .Returns(_movie);

            var allCriteria = WatchForSearchCriteria();

            Subject.MovieSearch(_movie, true, false);

            var criteria = allCriteria.OfType<MovieSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }
    }
}
