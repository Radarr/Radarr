using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.TorrentRss;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class EarlyReleaseSpecificationFixture : TestBase<EarlyReleaseSpecification>
    {
        private Artist _artist;
        private Album _album1;
        private Album _album2;
        private RemoteAlbum _remoteAlbum;
        private IndexerDefinition _indexerDefinition;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew().With(s => s.Id = 1).Build();
            _album1 = Builder<Album>.CreateNew().With(s => s.ReleaseDate = DateTime.Today).Build();
            _album2 = Builder<Album>.CreateNew().With(s => s.ReleaseDate = DateTime.Today).Build();

            _remoteAlbum = new RemoteAlbum
            {
                Artist = _artist,
                Albums = new List<Album>{_album1},
                Release = new TorrentInfo
                {
                    IndexerId = 1,
                    Title = "Artist - Album [FLAC-RlsGrp]",
                    PublishDate = DateTime.Today
                }
            };

            _indexerDefinition = new IndexerDefinition
            {
                Settings = new TorrentRssIndexerSettings { EarlyReleaseLimit = 5 }
            };

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(v => v.Get(1))
                  .Returns(_indexerDefinition);

        }

        private void GivenPublishDateFromToday(int days)
        {
            (_remoteAlbum.Release).PublishDate = DateTime.Today.AddDays(days);
        }

        [Test]
        public void should_return_true_if_indexer_not_specified()
        {
            _remoteAlbum.Release.IndexerId = 0;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_release_contains_multiple_albums()
        {
            _remoteAlbum.Albums.Add(_album2);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_indexer_no_longer_exists()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(v => v.Get(It.IsAny<int>()))
                  .Callback<int>(i => { throw new ModelNotFoundException(typeof(IndexerDefinition), i); });

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [TestCase(-2)]
        [TestCase(-5)]
        public void should_return_true_if_publish_date_above_or_equal_to_limit(int days)
        {
            GivenPublishDateFromToday(days);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [TestCase(-10)]
        [TestCase(-20)]
        public void should_return_false_if_publish_date_belove_limit(int days)
        {
            GivenPublishDateFromToday(days);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [TestCase(-10)]
        [TestCase(-100)]
        public void should_return_true_if_limit_null(int days)
        {
            GivenPublishDateFromToday(days);

            _indexerDefinition.Settings = new TorrentRssIndexerSettings{EarlyReleaseLimit = null};

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }
    }
}
