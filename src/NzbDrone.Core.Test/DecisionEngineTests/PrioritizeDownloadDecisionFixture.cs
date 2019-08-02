using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Music;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.DecisionEngine;
using NUnit.Framework;
using FluentAssertions;
using FizzWare.NBuilder;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class PrioritizeDownloadDecisionFixture : CoreTest<DownloadDecisionPriorizationService>
    {
        [SetUp]
        public void Setup()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);
        }

        private Album GivenAlbum(int id)
        {
            return Builder<Album>.CreateNew()
                            .With(e => e.Id = id)
                            .Build();
        }

        private RemoteAlbum GivenRemoteAlbum(List<Album> albums, QualityModel quality, int age = 0, long size = 0, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet)
        {
            var remoteAlbum = new RemoteAlbum();
            remoteAlbum.ParsedAlbumInfo = new ParsedAlbumInfo();
            remoteAlbum.ParsedAlbumInfo.Quality = quality;

            remoteAlbum.Albums = new List<Album>();
            remoteAlbum.Albums.AddRange(albums);

            remoteAlbum.Release = new ReleaseInfo();
            remoteAlbum.Release.PublishDate = DateTime.Now.AddDays(-age);
            remoteAlbum.Release.Size = size;
            remoteAlbum.Release.DownloadProtocol = downloadProtocol;

            remoteAlbum.Artist = Builder<Artist>.CreateNew()
                                                .With(e => e.QualityProfile = new QualityProfile
                                                {
                                                    Items = Qualities.QualityFixture.GetDefaultQualities()
                                                }).Build();

            remoteAlbum.DownloadAllowed = true;

            return remoteAlbum;
        }

        private void GivenPreferredDownloadProtocol(DownloadProtocol downloadProtocol)
        {
            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(new DelayProfile
                  {
                      PreferredProtocol = downloadProtocol
                  });
        }

        [Test]
        public void should_put_propers_before_non_propers()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256, new Revision(version: 1)));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256, new Revision(version: 2)));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_put_higher_quality_before_lower()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_192));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Quality.Quality.Should().Be(Quality.MP3_256);
        }

        [Test]
        public void should_order_by_age_then_largest_rounded_to_200mb()
        {
            var remoteAlbumSd = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_192), size: 100.Megabytes(), age: 1);
            var remoteAlbumHdSmallOld = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), size: 1200.Megabytes(), age: 1000);
            var remoteAlbumSmallYoung = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), size: 1250.Megabytes(), age: 10);
            var remoteAlbumHdLargeYoung = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), size: 3000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbumSd));
            decisions.Add(new DownloadDecision(remoteAlbumHdSmallOld));
            decisions.Add(new DownloadDecision(remoteAlbumSmallYoung));
            decisions.Add(new DownloadDecision(remoteAlbumHdLargeYoung));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.Should().Be(remoteAlbumHdLargeYoung);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), age: 10);
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), age: 5);


            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.Should().Be(remoteAlbum2);
        }

        [Test]
        public void should_not_throw_if_no_albums_are_found()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), size: 500.Megabytes());
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), size: 500.Megabytes());

            remoteAlbum1.Albums = new List<Album>();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            Subject.PrioritizeDecisions(decisions);
        }

        [Test]
        public void should_put_usenet_above_torrent_when_usenet_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);

            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), downloadProtocol: DownloadProtocol.Torrent);
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.Release.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
        }

        [Test]
        public void should_put_torrent_above_usenet_when_torrent_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Torrent);

            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), downloadProtocol: DownloadProtocol.Torrent);
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.Release.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
        }

        [Test]
        public void should_prefer_discography_pack_above_single_album()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1), GivenAlbum(2) }, new QualityModel(Quality.FLAC));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC));

            remoteAlbum1.ParsedAlbumInfo.Discography = true;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Discography.Should().BeTrue();
        }

        [Test]
        public void should_prefer_quality_over_discography_pack()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1), GivenAlbum(2) }, new QualityModel(Quality.MP3_320));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC));

            remoteAlbum1.ParsedAlbumInfo.Discography = true;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Discography.Should().BeFalse();
        }

        [Test]
        public void should_prefer_single_album_over_multi_album()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1), GivenAlbum(2) }, new QualityModel(Quality.MP3_256));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.Albums.Count.Should().Be(remoteAlbum2.Albums.Count);
        }

        [Test]
        public void should_prefer_releases_with_more_seeders()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 100;

            remoteAlbum1.Release = torrentInfo1;
            remoteAlbum2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteAlbum.Release).Seeders.Should().Be(torrentInfo2.Seeders);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_given_equal_number_of_seeds()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;
            torrentInfo1.Peers = 10;


            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Peers = 100;

            remoteAlbum1.Release = torrentInfo1;
            remoteAlbum2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteAlbum.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_no_seeds()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 0;
            torrentInfo1.Peers = 10;


            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 0;
            torrentInfo2.Peers = 100;

            remoteAlbum1.Release = torrentInfo1;
            remoteAlbum2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteAlbum.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_first_release_if_peers_and_size_are_too_similar()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 1000;
            torrentInfo1.Peers = 10;
            torrentInfo1.Size = 200.Megabytes();

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 1100;
            torrentInfo2.Peers = 10;
            torrentInfo1.Size = 250.Megabytes();

            remoteAlbum1.Release = torrentInfo1;
            remoteAlbum2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteAlbum.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_prefer_first_release_if_age_and_size_are_too_similar()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));

            remoteAlbum1.Release.PublishDate = DateTime.UtcNow.AddDays(-100);
            remoteAlbum1.Release.Size = 200.Megabytes();

            remoteAlbum2.Release.PublishDate = DateTime.UtcNow.AddDays(-150);
            remoteAlbum2.Release.Size = 250.Megabytes();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.Release.Should().Be(remoteAlbum1.Release);
        }

        [Test]
        public void should_prefer_quality_over_the_number_of_peers()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_192));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 100;
            torrentInfo1.Peers = 10;
            torrentInfo1.Size = 200.Megabytes();

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 1100;
            torrentInfo2.Peers = 10;
            torrentInfo1.Size = 250.Megabytes();

            remoteAlbum1.Release = torrentInfo1;
            remoteAlbum2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteAlbum.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_put_higher_quality_before_lower_always()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_256));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Quality.Quality.Should().Be(Quality.MP3_320);
        }


        [Test]
        public void should_prefer_higher_score_over_lower_score()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC));

            remoteAlbum1.PreferredWordScore = 10;
            remoteAlbum2.PreferredWordScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.PreferredWordScore.Should().Be(10);
        }

        [Test]
        public void should_prefer_proper_over_score_when_download_propers_is_prefer_and_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(1)));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(2)));

            remoteAlbum1.PreferredWordScore = 10;
            remoteAlbum2.PreferredWordScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_prefer_proper_over_score_when_download_propers_is_do_not_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotUpgrade);

            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(1)));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(2)));

            remoteAlbum1.PreferredWordScore = 10;
            remoteAlbum2.PreferredWordScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_prefer_score_over_proper_when_download_propers_is_do_not_prefer()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            var remoteAlbum1 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(1)));
            var remoteAlbum2 = GivenRemoteAlbum(new List<Album> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(2)));

            remoteAlbum1.PreferredWordScore = 10;
            remoteAlbum2.PreferredWordScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Quality.Quality.Should().Be(Quality.FLAC);
            qualifiedReports.First().RemoteAlbum.ParsedAlbumInfo.Quality.Revision.Version.Should().Be(1);
            qualifiedReports.First().RemoteAlbum.PreferredWordScore.Should().Be(10);
        }
    }
}
