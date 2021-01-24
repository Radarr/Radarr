using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class AuthorSpecificationFixture : TestBase<AuthorSpecification>
    {
        private Author _author1;
        private Author _author2;
        private RemoteBook _remoteBook = new RemoteBook();
        private SearchCriteriaBase _searchCriteria = new BookSearchCriteria();

        [SetUp]
        public void Setup()
        {
            _author1 = Builder<Author>.CreateNew().With(s => s.Id = 1).Build();
            _author2 = Builder<Author>.CreateNew().With(s => s.Id = 2).Build();

            _remoteBook.Author = _author1;
        }

        [Test]
        public void should_return_false_if_author_doesnt_match()
        {
            _searchCriteria.Author = _author2;

            Subject.IsSatisfiedBy(_remoteBook, _searchCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_author_ids_match()
        {
            _searchCriteria.Author = _author1;

            Subject.IsSatisfiedBy(_remoteBook, _searchCriteria).Accepted.Should().BeTrue();
        }
    }
}
