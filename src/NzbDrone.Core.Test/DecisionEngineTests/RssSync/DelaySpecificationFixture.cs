using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DelaySpecificationFixture : CoreTest<DelaySpecification>
    {
        private QualityProfile _profile;
        private DelayProfile _delayProfile;
        private RemoteAlbum _remoteAlbum;

        [SetUp]
        public void Setup()
        {
            _profile = Builder<QualityProfile>.CreateNew()
                                       .Build();


            _delayProfile = Builder<DelayProfile>.CreateNew()
                                      .With(d => d.PreferredProtocol = DownloadProtocol.Usenet)
                                      .Build();

            var artist = Builder<Artist>.CreateNew()
                                        .With(s => s.QualityProfile = _profile)
                                        .Build();

            _remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                   .With(r => r.Artist = artist)
                                                   .Build();

            _profile.Items = new List<QualityProfileQualityItem>();
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_256 });
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_320 });
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_320 });

            _profile.Cutoff = Quality.MP3_320.Id;

            _remoteAlbum.ParsedAlbumInfo = new ParsedAlbumInfo();
            _remoteAlbum.Release = new ReleaseInfo();
            _remoteAlbum.Release.DownloadProtocol = DownloadProtocol.Usenet;

            _remoteAlbum.Albums = Builder<Album>.CreateListOfSize(1).Build().ToList();

            Mocker.GetMock<IMediaFileService>()
                .Setup(s => s.GetFilesByAlbum(It.IsAny<int>()))
                .Returns(new List<TrackFile> { });

            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(_delayProfile);

            Mocker.GetMock<IPendingReleaseService>()
                  .Setup(s => s.GetPendingRemoteAlbums(It.IsAny<int>()))
                  .Returns(new List<RemoteAlbum>());
        }

        private void GivenExistingFile(QualityModel quality)
        {
            Mocker.GetMock<IMediaFileService>()
                .Setup(s => s.GetFilesByAlbum(It.IsAny<int>()))
                .Returns(new List<TrackFile> { new TrackFile {
                                                                Quality = quality
                                                              } });
        }

        private void GivenUpgradeForExistingFile()
        {
            Mocker.GetMock<IUpgradableSpecification>()
                  .Setup(s => s.IsUpgradable(It.IsAny<QualityProfile>(), It.IsAny<List<QualityModel>>(), It.IsAny<int>(), It.IsAny<QualityModel>(), It.IsAny<int>()))
                  .Returns(true);
        }

        [Test]
        public void should_be_true_when_user_invoked_search()
        {
            Subject.IsSatisfiedBy(new RemoteAlbum(), new AlbumSearchCriteria { UserInvokedSearch = true }).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_system_invoked_search_and_release_is_younger_than_delay()
        {
            _remoteAlbum.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_192);
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteAlbum, new AlbumSearchCriteria()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_profile_does_not_have_a_delay()
        {
            _delayProfile.UsenetDelay = 0;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_quality_is_last_allowed_in_profile()
        {
            _remoteAlbum.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_320);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_is_older_than_delay()
        {
            _remoteAlbum.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_256);
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow.AddHours(-10);

            _delayProfile.UsenetDelay = 60;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_release_is_younger_than_delay()
        {
            _remoteAlbum.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_192);
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_release_is_a_proper_for_existing_album()
        {
            _remoteAlbum.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2));
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.MP3_256));
            GivenUpgradeForExistingFile();

            Mocker.GetMock<IUpgradableSpecification>()
                  .Setup(s => s.IsRevisionUpgrade(It.IsAny<QualityModel>(), It.IsAny<QualityModel>()))
                  .Returns(true);

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_is_a_real_for_existing_album()
        {
            _remoteAlbum.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_256, new Revision(real: 1));
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.MP3_256));
            GivenUpgradeForExistingFile();

            Mocker.GetMock<IUpgradableSpecification>()
                  .Setup(s => s.IsRevisionUpgrade(It.IsAny<QualityModel>(), It.IsAny<QualityModel>()))
                  .Returns(true);

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_release_is_proper_for_existing_album_of_different_quality()
        {
            _remoteAlbum.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2));
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.MP3_192));

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
