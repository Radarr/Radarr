using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.TorrentRss;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class TorrentSeedingSpecificationFixture : TestBase<TorrentSeedingSpecification>
    {
        private Author _author;
        private RemoteBook _remoteBook;
        private IndexerDefinition _indexerDefinition;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew().With(s => s.Id = 1).Build();

            _remoteBook = new RemoteBook
            {
                Author = _author,
                Release = new TorrentInfo
                {
                    IndexerId = 1,
                    Title = "Author - Book [FLAC-RlsGrp]",
                    Seeders = 0
                }
            };

            _indexerDefinition = new IndexerDefinition
            {
                Settings = new TorrentRssIndexerSettings { MinimumSeeders = 5 }
            };

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(v => v.Get(1))
                  .Returns(_indexerDefinition);
        }

        private void GivenReleaseSeeders(int? seeders)
        {
            (_remoteBook.Release as TorrentInfo).Seeders = seeders;
        }

        [Test]
        public void should_return_true_if_not_torrent()
        {
            _remoteBook.Release = new ReleaseInfo
            {
                IndexerId = 1,
                Title = "Author - Book [FLAC-RlsGrp]"
            };

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_indexer_not_specified()
        {
            _remoteBook.Release.IndexerId = 0;

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_indexer_no_longer_exists()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(v => v.Get(It.IsAny<int>()))
                  .Callback<int>(i => { throw new ModelNotFoundException(typeof(IndexerDefinition), i); });

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_seeds_unknown()
        {
            GivenReleaseSeeders(null);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [TestCase(5)]
        [TestCase(6)]
        public void should_return_true_if_seeds_above_or_equal_to_limit(int seeders)
        {
            GivenReleaseSeeders(seeders);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(4)]
        public void should_return_false_if_seeds_belove_limit(int seeders)
        {
            GivenReleaseSeeders(seeders);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }
    }
}
