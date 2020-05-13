using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DelaySpecificationFixture : CoreTest<DelaySpecification>
    {
        private QualityProfile _profile;
        private DelayProfile _delayProfile;
        private RemoteBook _remoteAlbum;

        [SetUp]
        public void Setup()
        {
            _profile = Builder<QualityProfile>.CreateNew()
                                       .Build();

            _delayProfile = Builder<DelayProfile>.CreateNew()
                                      .With(d => d.PreferredProtocol = DownloadProtocol.Usenet)
                                      .Build();

            var artist = Builder<Author>.CreateNew()
                                        .With(s => s.QualityProfile = _profile)
                                        .Build();

            _remoteAlbum = Builder<RemoteBook>.CreateNew()
                                                   .With(r => r.Author = artist)
                                                   .Build();

            _profile.Items = new List<QualityProfileQualityItem>();
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.PDF });
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.AZW3 });
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.MP3_320 });

            _profile.Cutoff = Quality.AZW3.Id;

            _remoteAlbum.ParsedBookInfo = new ParsedBookInfo();
            _remoteAlbum.Release = new ReleaseInfo();
            _remoteAlbum.Release.DownloadProtocol = DownloadProtocol.Usenet;

            _remoteAlbum.Books = Builder<Book>.CreateListOfSize(1).Build().ToList();

            Mocker.GetMock<IMediaFileService>()
                .Setup(s => s.GetFilesByBook(It.IsAny<int>()))
                .Returns(new List<BookFile> { });

            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(_delayProfile);

            Mocker.GetMock<IPendingReleaseService>()
                  .Setup(s => s.GetPendingRemoteBooks(It.IsAny<int>()))
                  .Returns(new List<RemoteBook>());
        }

        private void GivenExistingFile(QualityModel quality)
        {
            Mocker.GetMock<IMediaFileService>()
                .Setup(s => s.GetFilesByBook(It.IsAny<int>()))
                .Returns(new List<BookFile>
                {
                    new BookFile
                    {
                        Quality = quality
                    }
                });
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
            Subject.IsSatisfiedBy(new RemoteBook(), new BookSearchCriteria { UserInvokedSearch = true }).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_system_invoked_search_and_release_is_younger_than_delay()
        {
            _remoteAlbum.ParsedBookInfo.Quality = new QualityModel(Quality.MOBI);
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteAlbum, new BookSearchCriteria()).Accepted.Should().BeFalse();
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
            _remoteAlbum.ParsedBookInfo.Quality = new QualityModel(Quality.MP3_320);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_is_older_than_delay()
        {
            _remoteAlbum.ParsedBookInfo.Quality = new QualityModel(Quality.MOBI);
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow.AddHours(-10);

            _delayProfile.UsenetDelay = 60;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_release_is_younger_than_delay()
        {
            _remoteAlbum.ParsedBookInfo.Quality = new QualityModel(Quality.MOBI);
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_release_is_a_proper_for_existing_album()
        {
            _remoteAlbum.ParsedBookInfo.Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2));
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.MP3_320));
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
            _remoteAlbum.ParsedBookInfo.Quality = new QualityModel(Quality.MP3_320, new Revision(real: 1));
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.MP3_320));
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
            _remoteAlbum.ParsedBookInfo.Quality = new QualityModel(Quality.AZW3, new Revision(version: 2));
            _remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.PDF));

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
