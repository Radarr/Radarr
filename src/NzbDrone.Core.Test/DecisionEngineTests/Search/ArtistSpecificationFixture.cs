using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class ArtistSpecificationFixture : TestBase<ArtistSpecification>
    {
        private Artist _artist1;
        private Artist _artist2;
        private RemoteAlbum _remoteAlbum = new RemoteAlbum();
        private SearchCriteriaBase _searchCriteria = new AlbumSearchCriteria();

        [SetUp]
        public void Setup()
        {
            _artist1 = Builder<Artist>.CreateNew().With(s => s.Id = 1).Build();
            _artist2 = Builder<Artist>.CreateNew().With(s => s.Id = 2).Build();

            _remoteAlbum.Artist = _artist1;
        }

        [Test]
        public void should_return_false_if_artist_doesnt_match()
        {
            _searchCriteria.Artist = _artist2;

            Subject.IsSatisfiedBy(_remoteAlbum, _searchCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_artist_ids_match()
        {
            _searchCriteria.Artist = _artist1;

            Subject.IsSatisfiedBy(_remoteAlbum, _searchCriteria).Accepted.Should().BeTrue();
        }
    }
}
