using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class HistorySpecificationFixture : CoreTest<HistorySpecification>
    {
        private const int FIRST_EPISODE_ID = 1;
        private const int SECOND_EPISODE_ID = 2;

        private HistorySpecification _upgradeHistory;

        private RemoteMovie _parseResultSingle;
        private QualityModel _upgradableQuality;
        private QualityModel _notupgradableQuality;
        private Movie _fakeMovie;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<HistorySpecification>();

            CustomFormatsFixture.GivenCustomFormats();

            _fakeMovie = Builder<Movie>.CreateNew()
                .With(c => c.Profile = new Profile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    Cutoff = Quality.Bluray1080p.Id,
                    FormatItems = CustomFormatsFixture.GetSampleFormatItems("None"),
                    MinFormatScore = 0,
                    UpgradeAllowed = true
                })
                .Build();

            _parseResultSingle = new RemoteMovie
            {
                Movie = _fakeMovie,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                CustomFormats = new List<CustomFormat>()
            };

            _upgradableQuality = new QualityModel(Quality.SDTV, new Revision(version: 1));
            _notupgradableQuality = new QualityModel(Quality.HDTV1080p, new Revision(version: 2));

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<MovieHistory>()))
                .Returns(new List<CustomFormat>());
        }

        private void GivenMostRecentForEpisode(int episodeId, string downloadId, QualityModel quality, DateTime date, MovieHistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForMovie(episodeId))
                  .Returns(new MovieHistory { DownloadId = downloadId, Quality = quality, Date = date, EventType = eventType });
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
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new MovieSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_null()
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForMovie(It.IsAny<int>())).Returns((MovieHistory)null);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_not_grabbed()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, MovieHistoryEventType.DownloadFailed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
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
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-13), MovieHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, MovieHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        /*
        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_episodes_are_not_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }*/

        [Test]
        public void should_not_be_upgradable_if_episode_is_of_same_quality_as_existing()
        {
            _fakeMovie.Profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                Cutoff = Quality.Bluray1080p.Id,
                FormatItems = CustomFormatsFixture.GetSampleFormatItems(),
                MinFormatScore = 0
            };

            _parseResultSingle.ParsedMovieInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<MovieHistory>()))
                .Returns(new List<CustomFormat>());

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, MovieHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_cutoff_already_met()
        {
            _fakeMovie.Profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                Cutoff = Quality.WEBDL1080p.Id,
                FormatItems = CustomFormatsFixture.GetSampleFormatItems(),
                MinFormatScore = 0
            };

            _parseResultSingle.ParsedMovieInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1));

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, MovieHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_item_is_only_one_hour_old()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-1), MovieHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_has_a_download_id_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), MovieHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_already_met_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            _fakeMovie.Profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                Cutoff = Quality.WEBDL1080p.Id,
                FormatItems = CustomFormatsFixture.GetSampleFormatItems(),
                MinFormatScore = 0
            };

            _parseResultSingle.ParsedMovieInfo.Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), MovieHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_only_episode_is_not_upgradable_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow.AddDays(-100), MovieHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }
    }
}
