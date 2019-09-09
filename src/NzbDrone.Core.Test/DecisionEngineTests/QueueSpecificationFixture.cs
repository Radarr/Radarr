using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class QueueSpecificationFixture : CoreTest<QueueSpecification>
    {
        private Artist _artist;
        private Album _album;
        private RemoteAlbum _remoteAlbum;

        private Artist _otherArtist;
        private Album _otherAlbum;

        private ReleaseInfo _releaseInfo;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _artist = Builder<Artist>.CreateNew()
                                     .With(e => e.QualityProfile = new QualityProfile
                                     {
                                         UpgradeAllowed = true,
                                         Items = Qualities.QualityFixture.GetDefaultQualities(),
                                     })
                                     .Build();

            _album = Builder<Album>.CreateNew()
                                       .With(e => e.ArtistId = _artist.Id)
                                       .Build();

            _otherArtist = Builder<Artist>.CreateNew()
                                          .With(s => s.Id = 2)
                                          .Build();

            _otherAlbum = Builder<Album>.CreateNew()
                                            .With(e => e.ArtistId = _otherArtist.Id)
                                            .With(e => e.Id = 2)
                                            .Build();

            _releaseInfo = Builder<ReleaseInfo>.CreateNew()
                                   .Build();

            _remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                   .With(r => r.Artist = _artist)
                                                   .With(r => r.Albums = new List<Album> { _album })
                                                   .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_256) })
                                                   .With(r => r.PreferredWordScore = 0)
                                                   .Build();
        }

        private void GivenEmptyQueue()
        {
            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(new List<Queue.Queue>());
        }

        private void GivenQueue(IEnumerable<RemoteAlbum> remoteAlbums)
        {
            var queue = remoteAlbums.Select(remoteAlbum => new Queue.Queue
            {
                RemoteAlbum = remoteAlbum
            });

            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(queue.ToList());
        }

        [Test]
        public void should_return_true_when_queue_is_empty()
        {
            GivenEmptyQueue();
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_artist_doesnt_match()
        {
            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                       .With(r => r.Artist = _otherArtist)
                                                       .With(r => r.Albums = new List<Album> { _album })
                                                       .With(r => r.Release = _releaseInfo)
                                                       .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_everything_is_the_same()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.FLAC.Id;

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                .With(r => r.Artist = _artist)
                .With(r => r.Albums = new List<Album> { _album })
                .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                {
                    Quality = new QualityModel(Quality.MP3_256)
                })
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_lower()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.MP3_320.Id;

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(r => r.Artist = _artist)
                                                      .With(r => r.Albums = new List<Album> { _album })
                                                      .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_192)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_album_doesnt_match()
        {
            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(r => r.Artist = _artist)
                                                      .With(r => r.Albums = new List<Album> { _otherAlbum })
                                                      .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_192)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_qualities_are_the_same_with_higher_preferred_word_score()
        {
            _remoteAlbum.PreferredWordScore = 1;

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                .With(r => r.Artist = _artist)
                .With(r => r.Albums = new List<Album> { _album })
                .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                {
                    Quality = new QualityModel(Quality.MP3_256)
                })
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_qualities_are_the_same()
        {
            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(r => r.Artist = _artist)
                                                      .With(r => r.Albums = new List<Album> { _album })
                                                      .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_192)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_in_queue_is_better()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.FLAC.Id;

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(r => r.Artist = _artist)
                                                      .With(r => r.Albums = new List<Album> { _album })
                                                      .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_matching_multi_album_is_in_queue()
        {
            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(r => r.Artist = _artist)
                                                      .With(r => r.Albums = new List<Album> { _album, _otherAlbum })
                                                      .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_album_has_one_album_in_queue()
        {
            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(r => r.Artist = _artist)
                                                      .With(r => r.Albums = new List<Album> { _album })
                                                      .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            _remoteAlbum.Albums.Add(_otherAlbum);

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_part_album_is_already_in_queue()
        {
            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(r => r.Artist = _artist)
                                                      .With(r => r.Albums = new List<Album> { _album, _otherAlbum })
                                                      .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            _remoteAlbum.Albums.Add(_otherAlbum);

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_part_album_has_two_albums_in_queue()
        {
            var remoteAlbums = Builder<RemoteAlbum>.CreateListOfSize(2)
                                                       .All()
                                                       .With(r => r.Artist = _artist)
                                                       .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                                                       {
                                                           Quality = new QualityModel(Quality.MP3_320)
                                                       })
                                                       .With(r => r.Release = _releaseInfo)
                                                       .TheFirst(1)
                                                       .With(r => r.Albums = new List<Album> { _album })
                                                       .TheNext(1)
                                                       .With(r => r.Albums = new List<Album> { _otherAlbum })
                                                       .Build();

            _remoteAlbum.Albums.Add(_otherAlbum);
            GivenQueue(remoteAlbums);
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_is_better_and_upgrade_allowed_is_false_for_quality_profile()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.FLAC.Id;
            _artist.QualityProfile.Value.UpgradeAllowed = false;

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                .With(r => r.Artist = _artist)
                .With(r => r.Albums = new List<Album> { _album })
                .With(r => r.ParsedAlbumInfo = new ParsedAlbumInfo
                {
                    Quality = new QualityModel(Quality.FLAC)
                })
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteAlbum> { remoteAlbum });
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
