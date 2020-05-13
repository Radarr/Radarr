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
    public class ArtistSpecificationFixture : TestBase<AuthorSpecification>
    {
        private Author _artist1;
        private Author _artist2;
        private RemoteBook _remoteAlbum = new RemoteBook();
        private SearchCriteriaBase _searchCriteria = new BookSearchCriteria();

        [SetUp]
        public void Setup()
        {
            _artist1 = Builder<Author>.CreateNew().With(s => s.Id = 1).Build();
            _artist2 = Builder<Author>.CreateNew().With(s => s.Id = 2).Build();

            _remoteAlbum.Author = _artist1;
        }

        [Test]
        public void should_return_false_if_artist_doesnt_match()
        {
            _searchCriteria.Author = _artist2;

            Subject.IsSatisfiedBy(_remoteAlbum, _searchCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_artist_ids_match()
        {
            _searchCriteria.Author = _artist1;

            Subject.IsSatisfiedBy(_remoteAlbum, _searchCriteria).Accepted.Should().BeTrue();
        }
    }
}
