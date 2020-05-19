using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class QueueSpecificationFixture : CoreTest<QueueSpecification>
    {
        private Author _artist;
        private Book _album;
        private RemoteBook _remoteBook;

        private Author _otherArtist;
        private Book _otherAlbum;

        private ReleaseInfo _releaseInfo;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _artist = Builder<Author>.CreateNew()
                                     .With(e => e.QualityProfile = new QualityProfile
                                     {
                                         UpgradeAllowed = true,
                                         Items = Qualities.QualityFixture.GetDefaultQualities(),
                                     })
                                     .Build();

            _album = Builder<Book>.CreateNew()
                                       .With(e => e.AuthorId = _artist.Id)
                                       .Build();

            _otherArtist = Builder<Author>.CreateNew()
                                          .With(s => s.Id = 2)
                                          .Build();

            _otherAlbum = Builder<Book>.CreateNew()
                                            .With(e => e.AuthorId = _otherArtist.Id)
                                            .With(e => e.Id = 2)
                                            .Build();

            _releaseInfo = Builder<ReleaseInfo>.CreateNew()
                                   .Build();

            _remoteBook = Builder<RemoteBook>.CreateNew()
                                                   .With(r => r.Author = _artist)
                                                   .With(r => r.Books = new List<Book> { _album })
                                                   .With(r => r.ParsedBookInfo = new ParsedBookInfo { Quality = new QualityModel(Quality.MP3_320) })
                                                   .With(r => r.PreferredWordScore = 0)
                                                   .Build();
        }

        private void GivenEmptyQueue()
        {
            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(new List<Queue.Queue>());
        }

        private void GivenQueue(IEnumerable<RemoteBook> remoteBooks, TrackedDownloadState trackedDownloadState = TrackedDownloadState.Downloading)
        {
            var queue = remoteBooks.Select(remoteBook => new Queue.Queue
            {
                RemoteBook = remoteBook,
                TrackedDownloadState = trackedDownloadState
            });

            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(queue.ToList());
        }

        [Test]
        public void should_return_true_when_queue_is_empty()
        {
            GivenEmptyQueue();
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_artist_doesnt_match()
        {
            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                       .With(r => r.Author = _otherArtist)
                                                       .With(r => r.Books = new List<Book> { _album })
                                                       .With(r => r.Release = _releaseInfo)
                                                       .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_everything_is_the_same()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.FLAC.Id;

            var remoteBook = Builder<RemoteBook>.CreateNew()
                .With(r => r.Author = _artist)
                .With(r => r.Books = new List<Book> { _album })
                .With(r => r.ParsedBookInfo = new ParsedBookInfo
                {
                    Quality = new QualityModel(Quality.MP3_320)
                })
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_lower()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.MP3_320.Id;

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(r => r.Author = _artist)
                                                      .With(r => r.Books = new List<Book> { _album })
                                                      .With(r => r.ParsedBookInfo = new ParsedBookInfo
                                                      {
                                                          Quality = new QualityModel(Quality.AZW3)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_album_doesnt_match()
        {
            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(r => r.Author = _artist)
                                                      .With(r => r.Books = new List<Book> { _otherAlbum })
                                                      .With(r => r.ParsedBookInfo = new ParsedBookInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_qualities_are_the_same_with_higher_preferred_word_score()
        {
            _remoteBook.PreferredWordScore = 1;

            var remoteBook = Builder<RemoteBook>.CreateNew()
                .With(r => r.Author = _artist)
                .With(r => r.Books = new List<Book> { _album })
                .With(r => r.ParsedBookInfo = new ParsedBookInfo
                {
                    Quality = new QualityModel(Quality.MP3_320)
                })
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_qualities_are_the_same()
        {
            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(r => r.Author = _artist)
                                                      .With(r => r.Books = new List<Book> { _album })
                                                      .With(r => r.ParsedBookInfo = new ParsedBookInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_in_queue_is_better()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.FLAC.Id;

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(r => r.Author = _artist)
                                                      .With(r => r.Books = new List<Book> { _album })
                                                      .With(r => r.ParsedBookInfo = new ParsedBookInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_matching_multi_album_is_in_queue()
        {
            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(r => r.Author = _artist)
                                                      .With(r => r.Books = new List<Book> { _album, _otherAlbum })
                                                      .With(r => r.ParsedBookInfo = new ParsedBookInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_album_has_one_album_in_queue()
        {
            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(r => r.Author = _artist)
                                                      .With(r => r.Books = new List<Book> { _album })
                                                      .With(r => r.ParsedBookInfo = new ParsedBookInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            _remoteBook.Books.Add(_otherAlbum);

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_part_album_is_already_in_queue()
        {
            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(r => r.Author = _artist)
                                                      .With(r => r.Books = new List<Book> { _album, _otherAlbum })
                                                      .With(r => r.ParsedBookInfo = new ParsedBookInfo
                                                      {
                                                          Quality = new QualityModel(Quality.MP3_320)
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .Build();

            _remoteBook.Books.Add(_otherAlbum);

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_part_album_has_two_albums_in_queue()
        {
            var remoteBooks = Builder<RemoteBook>.CreateListOfSize(2)
                                                       .All()
                                                       .With(r => r.Author = _artist)
                                                       .With(r => r.ParsedBookInfo = new ParsedBookInfo
                                                       {
                                                           Quality = new QualityModel(Quality.MP3_320)
                                                       })
                                                       .With(r => r.Release = _releaseInfo)
                                                       .TheFirst(1)
                                                       .With(r => r.Books = new List<Book> { _album })
                                                       .TheNext(1)
                                                       .With(r => r.Books = new List<Book> { _otherAlbum })
                                                       .Build();

            _remoteBook.Books.Add(_otherAlbum);
            GivenQueue(remoteBooks);
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_is_better_and_upgrade_allowed_is_false_for_quality_profile()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.FLAC.Id;
            _artist.QualityProfile.Value.UpgradeAllowed = false;

            var remoteBook = Builder<RemoteBook>.CreateNew()
                .With(r => r.Author = _artist)
                .With(r => r.Books = new List<Book> { _album })
                .With(r => r.ParsedBookInfo = new ParsedBookInfo
                {
                    Quality = new QualityModel(Quality.FLAC)
                })
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteBook> { remoteBook });
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_everything_is_the_same_for_failed_pending()
        {
            _artist.QualityProfile.Value.Cutoff = Quality.FLAC.Id;

            var remoteBook = Builder<RemoteBook>.CreateNew()
                .With(r => r.Author = _artist)
                .With(r => r.Books = new List<Book> { _album })
                .With(r => r.ParsedBookInfo = new ParsedBookInfo
                {
                    Quality = new QualityModel(Quality.MP3_320)
                })
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteBook> { remoteBook }, TrackedDownloadState.DownloadFailedPending);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }
    }
}
