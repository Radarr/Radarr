using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class HistorySpecificationFixture : CoreTest<HistorySpecification>
    {
        private const int FIRST_ALBUM_ID = 1;
        private const int SECOND_ALBUM_ID = 2;

        private HistorySpecification _upgradeHistory;

        private RemoteBook _parseResultMulti;
        private RemoteBook _parseResultSingle;
        private QualityModel _upgradableQuality;
        private QualityModel _notupgradableQuality;
        private Author _fakeAuthor;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<HistorySpecification>();

            var singleBookList = new List<Book> { new Book { Id = FIRST_ALBUM_ID } };
            var doubleBookList = new List<Book>
            {
                                                            new Book { Id = FIRST_ALBUM_ID },
                                                            new Book { Id = SECOND_ALBUM_ID },
                                                            new Book { Id = 3 }
            };

            _fakeAuthor = Builder<Author>.CreateNew()
                .With(c => c.QualityProfile = new QualityProfile
                {
                    UpgradeAllowed = true,
                    Cutoff = Quality.MP3_320.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                })
                .Build();

            _parseResultMulti = new RemoteBook
            {
                Author = _fakeAuthor,
                ParsedBookInfo = new ParsedBookInfo { Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                Books = doubleBookList
            };

            _parseResultSingle = new RemoteBook
            {
                Author = _fakeAuthor,
                ParsedBookInfo = new ParsedBookInfo { Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                Books = singleBookList
            };

            _upgradableQuality = new QualityModel(Quality.MP3_320, new Revision(version: 1));
            _notupgradableQuality = new QualityModel(Quality.MP3_320, new Revision(version: 2));

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);
        }

        private void GivenMostRecentForBook(int bookId, string downloadId, QualityModel quality, DateTime date, HistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForBook(bookId))
                  .Returns(new History.History { DownloadId = downloadId, Quality = quality, Date = date, EventType = eventType });
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        [Test]
        public void should_return_true_if_it_is_a_search()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new BookSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_null()
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForBook(It.IsAny<int>())).Returns((History.History)null);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_not_grabbed()
        {
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.DownloadFailed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        //        [Test]
        //        public void should_return_true_if_latest_history_has_a_download_id_and_cdh_is_enabled()
        //        {
        //            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
        //            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        //        }
        [Test]
        public void should_return_true_if_latest_history_item_is_older_than_twelve_hours()
        {
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-12).AddMilliseconds(-1), HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_book_is_upgradable()
        {
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_books_are_upgradable()
        {
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForBook(SECOND_ALBUM_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_books_are_not_upgradable()
        {
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForBook(SECOND_ALBUM_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_books_is_upgradable()
        {
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_books_is_upgradable()
        {
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForBook(SECOND_ALBUM_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_book_is_of_same_quality_as_existing()
        {
            _fakeAuthor.QualityProfile = new QualityProfile { Cutoff = Quality.MP3_320.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedBookInfo.Quality = new QualityModel(Quality.MP3_320, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.MP3_320, new Revision(version: 1));

            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_cutoff_already_met()
        {
            _fakeAuthor.QualityProfile = new QualityProfile { Cutoff = Quality.MP3_320.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedBookInfo.Quality = new QualityModel(Quality.MP3_320, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.MP3_320, new Revision(version: 1));

            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_item_is_only_one_hour_old()
        {
            GivenMostRecentForBook(FIRST_ALBUM_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-1), HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_has_a_download_id_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForBook(FIRST_ALBUM_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_already_met_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            _fakeAuthor.QualityProfile = new QualityProfile { Cutoff = Quality.MP3_320.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedBookInfo.Quality = new QualityModel(Quality.MP3_320, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.MP3_320, new Revision(version: 1));

            GivenMostRecentForBook(FIRST_ALBUM_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), HistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_only_book_is_not_upgradable_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForBook(FIRST_ALBUM_ID, "test", _notupgradableQuality, DateTime.UtcNow.AddDays(-100), HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }
    }
}
