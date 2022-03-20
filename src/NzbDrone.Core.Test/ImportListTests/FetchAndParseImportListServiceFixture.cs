using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests
{
    [TestFixture]
    public class FetchAndParseImportListServiceFixture : CoreTest<FetchAndParseImportListService>
    {
        private List<IImportList> _importLists;
        private List<ImportListStatus> _blockedLists;
        private List<ImportListMovie> _listMovies;

        [SetUp]
        public void Setup()
        {
            _importLists = new List<IImportList>();
            _blockedLists = new List<ImportListStatus>();

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.Enabled())
                  .Returns(_importLists);

            Mocker.GetMock<IImportListStatusService>()
                  .Setup(v => v.GetBlockedProviders())
                  .Returns(_blockedLists);

            _listMovies = Builder<ImportListMovie>.CreateListOfSize(5)
                .Build().ToList();

            Mocker.GetMock<ISearchForNewMovie>()
                .Setup(v => v.MapMovieToTmdbMovie(It.IsAny<MovieMetadata>()))
                .Returns<MovieMetadata>(m => new MovieMetadata { TmdbId = m.TmdbId });
        }

        private void GivenList(int id, bool enabled, bool enabledAuto, ImportListFetchResult fetchResult)
        {
            CreateListResult(id, enabled, enabledAuto, fetchResult);
        }

        private Mock<IImportList> CreateListResult(int id, bool enabled, bool enabledAuto, ImportListFetchResult fetchResult)
        {
            var importListDefinition = new ImportListDefinition { Id = id, EnableAuto = enabledAuto };

            var mockImportList = new Mock<IImportList>();
            mockImportList.SetupGet(s => s.Definition).Returns(importListDefinition);
            mockImportList.SetupGet(s => s.Enabled).Returns(enabled);
            mockImportList.SetupGet(s => s.EnableAuto).Returns(enabledAuto);
            mockImportList.Setup(s => s.Fetch()).Returns(fetchResult);

            _importLists.Add(mockImportList.Object);

            return mockImportList;
        }

        private void GivenBlockedList(int id)
        {
            _blockedLists.Add(new ImportListStatus { ProviderId = id, DisabledTill = DateTime.UtcNow.AddDays(2) });
        }

        [Test]
        public void should_return_failure_if_blocked_list()
        {
            var fetchResult = new ImportListFetchResult();
            GivenList(1, true, true, fetchResult);
            GivenBlockedList(1);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();
        }

        [Test]
        public void should_return_failure_if_one_blocked_list_one_good_list()
        {
            var fetchResult1 = new ImportListFetchResult();
            GivenList(1, true, true, fetchResult1);
            GivenBlockedList(1);

            var fetchResult2 = new ImportListFetchResult { Movies = _listMovies, AnyFailure = true };
            GivenList(2, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();
        }

        [Test]
        public void should_return_failure_if_single_list_fails()
        {
            var fetchResult = new ImportListFetchResult { Movies = _listMovies, AnyFailure = true };
            GivenList(1, true, true, fetchResult);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();
        }

        [Test]
        public void should_return_failure_if_any_list_fails()
        {
            var fetchResult1 = new ImportListFetchResult { Movies = _listMovies, AnyFailure = true };
            GivenList(1, true, true, fetchResult1);
            var fetchResult2 = new ImportListFetchResult { Movies = _listMovies, AnyFailure = false };
            GivenList(2, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();
        }

        [Test]
        public void should_return_early_if_no_available_lists()
        {
            var listResult = Subject.Fetch();

            Mocker.GetMock<IImportListStatusService>()
                  .Verify(v => v.GetBlockedProviders(), Times.Never());

            listResult.Movies.Count.Should().Be(0);
            listResult.AnyFailure.Should().BeFalse();
        }

        [Test]
        public void should_store_movies_if_list_doesnt_fail()
        {
            var listId = 1;
            var fetchResult = new ImportListFetchResult { Movies = _listMovies, AnyFailure = false };
            GivenList(listId, true, true, fetchResult);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeFalse();

            Mocker.GetMock<IImportListMovieService>()
                .Verify(v => v.SyncMoviesForList(It.IsAny<List<ImportListMovie>>(), listId), Times.Once());
        }

        [Test]
        public void should_not_store_movies_if_list_fails()
        {
            var listId = 1;
            var fetchResult = new ImportListFetchResult { Movies = _listMovies, AnyFailure = true };
            GivenList(listId, true, true, fetchResult);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();

            Mocker.GetMock<IImportListMovieService>()
                .Verify(v => v.SyncMoviesForList(It.IsAny<List<ImportListMovie>>(), listId), Times.Never());
        }

        [Test]
        public void should_only_store_movies_for_lists_that_dont_fail()
        {
            var passedListId = 1;
            var fetchResult1 = new ImportListFetchResult { Movies = _listMovies, AnyFailure = false };
            GivenList(passedListId, true, true, fetchResult1);
            var failedListId = 2;
            var fetchResult2 = new ImportListFetchResult { Movies = _listMovies, AnyFailure = true };
            GivenList(failedListId, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();

            Mocker.GetMock<IImportListMovieService>()
                .Verify(v => v.SyncMoviesForList(It.IsAny<List<ImportListMovie>>(), passedListId), Times.Once());
        }

        [Test]
        public void should_return_all_results_for_all_lists()
        {
            var passedListId = 1;
            var fetchResult1 = new ImportListFetchResult { Movies = _listMovies, AnyFailure = false };
            GivenList(passedListId, true, true, fetchResult1);
            var failedListId = 2;
            var fetchResult2 = new ImportListFetchResult { Movies = _listMovies, AnyFailure = false };
            GivenList(failedListId, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeFalse();
            listResult.Movies.Count.Should().Be(10);
        }
    }
}
