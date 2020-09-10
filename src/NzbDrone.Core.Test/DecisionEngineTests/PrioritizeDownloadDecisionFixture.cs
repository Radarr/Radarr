using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

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

        private Book GivenAlbum(int id)
        {
            return Builder<Book>.CreateNew()
                            .With(e => e.Id = id)
                            .Build();
        }

        private RemoteBook GivenRemoteAlbum(List<Book> albums, QualityModel quality, int age = 0, long size = 0, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet, int indexerPriority = 25)
        {
            var remoteBook = new RemoteBook();
            remoteBook.ParsedBookInfo = new ParsedBookInfo();
            remoteBook.ParsedBookInfo.Quality = quality;

            remoteBook.Books = new List<Book>();
            remoteBook.Books.AddRange(albums);

            remoteBook.Release = new ReleaseInfo();
            remoteBook.Release.PublishDate = DateTime.Now.AddDays(-age);
            remoteBook.Release.Size = size;
            remoteBook.Release.DownloadProtocol = downloadProtocol;
            remoteBook.Release.IndexerPriority = indexerPriority;

            remoteBook.Author = Builder<Author>.CreateNew()
                                                .With(e => e.QualityProfile = new QualityProfile
                                                {
                                                    Items = Qualities.QualityFixture.GetDefaultQualities()
                                                }).Build();

            remoteBook.DownloadAllowed = true;

            return remoteBook;
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
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320, new Revision(version: 1)));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320, new Revision(version: 2)));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_put_higher_quality_before_lower()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Quality.Quality.Should().Be(Quality.MP3_320);
        }

        [Test]
        public void should_order_by_age_then_largest_rounded_to_200mb()
        {
            var remoteBookSd = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), size: 100.Megabytes(), age: 1);
            var remoteBookHdSmallOld = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), size: 1200.Megabytes(), age: 1000);
            var remoteBookSmallYoung = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), size: 1250.Megabytes(), age: 10);
            var remoteBookHdLargeYoung = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), size: 3000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBookSd));
            decisions.Add(new DownloadDecision(remoteBookHdSmallOld));
            decisions.Add(new DownloadDecision(remoteBookSmallYoung));
            decisions.Add(new DownloadDecision(remoteBookHdLargeYoung));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.Should().Be(remoteBookHdLargeYoung);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), age: 10);
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), age: 5);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.Should().Be(remoteBook2);
        }

        [Test]
        public void should_not_throw_if_no_albums_are_found()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), size: 500.Megabytes());
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), size: 500.Megabytes());

            remoteBook1.Books = new List<Book>();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            Subject.PrioritizeDecisions(decisions);
        }

        [Test]
        public void should_put_usenet_above_torrent_when_usenet_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);

            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), downloadProtocol: DownloadProtocol.Torrent);
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.Release.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
        }

        [Test]
        public void should_put_torrent_above_usenet_when_torrent_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Torrent);

            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), downloadProtocol: DownloadProtocol.Torrent);
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.Release.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
        }

        [Test]
        public void should_prefer_discography_pack_above_single_album()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1), GivenAlbum(2) }, new QualityModel(Quality.FLAC));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC));

            remoteBook1.ParsedBookInfo.Discography = true;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Discography.Should().BeTrue();
        }

        [Test]
        public void should_prefer_quality_over_discography_pack()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1), GivenAlbum(2) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC));

            remoteBook1.ParsedBookInfo.Discography = true;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Discography.Should().BeFalse();
        }

        [Test]
        public void should_prefer_single_album_over_multi_album()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1), GivenAlbum(2) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.Books.Count.Should().Be(remoteBook2.Books.Count);
        }

        [Test]
        public void should_prefer_releases_with_more_seeders()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 100;

            remoteBook1.Release = torrentInfo1;
            remoteBook2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteBook.Release).Seeders.Should().Be(torrentInfo2.Seeders);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_given_equal_number_of_seeds()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;
            torrentInfo1.Peers = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Peers = 100;

            remoteBook1.Release = torrentInfo1;
            remoteBook2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteBook.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_no_seeds()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 0;
            torrentInfo1.Peers = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 0;
            torrentInfo2.Peers = 100;

            remoteBook1.Release = torrentInfo1;
            remoteBook2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteBook.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_first_release_if_peers_and_size_are_too_similar()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

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

            remoteBook1.Release = torrentInfo1;
            remoteBook2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteBook.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_prefer_first_release_if_age_and_size_are_too_similar()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

            remoteBook1.Release.PublishDate = DateTime.UtcNow.AddDays(-100);
            remoteBook1.Release.Size = 200.Megabytes();

            remoteBook2.Release.PublishDate = DateTime.UtcNow.AddDays(-150);
            remoteBook2.Release.Size = 250.Megabytes();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.Release.Should().Be(remoteBook1.Release);
        }

        [Test]
        public void should_prefer_quality_over_the_number_of_peers()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.AZW3));

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

            remoteBook1.Release = torrentInfo1;
            remoteBook2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteBook.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_put_higher_quality_before_lower_always()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Quality.Quality.Should().Be(Quality.MP3_320);
        }

        [Test]
        public void should_prefer_higher_score_over_lower_score()
        {
            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC));

            remoteBook1.PreferredWordScore = 10;
            remoteBook2.PreferredWordScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.PreferredWordScore.Should().Be(10);
        }

        [Test]
        public void should_prefer_proper_over_score_when_download_propers_is_prefer_and_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(1)));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(2)));

            remoteBook1.PreferredWordScore = 10;
            remoteBook2.PreferredWordScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_prefer_proper_over_score_when_download_propers_is_do_not_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotUpgrade);

            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(1)));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(2)));

            remoteBook1.PreferredWordScore = 10;
            remoteBook2.PreferredWordScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_prefer_score_over_proper_when_download_propers_is_do_not_prefer()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            var remoteBook1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(1)));
            var remoteBook2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.FLAC, new Revision(2)));

            remoteBook1.PreferredWordScore = 10;
            remoteBook2.PreferredWordScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteBook1));
            decisions.Add(new DownloadDecision(remoteBook2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Quality.Quality.Should().Be(Quality.FLAC);
            qualifiedReports.First().RemoteBook.ParsedBookInfo.Quality.Revision.Version.Should().Be(1);
            qualifiedReports.First().RemoteBook.PreferredWordScore.Should().Be(10);
        }

        [Test]
        public void sort_download_decisions_based_on_indexer_priority()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.AZW3, new Revision(1)), indexerPriority: 25);
            var remoteAlbum2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.AZW3, new Revision(1)), indexerPriority: 50);
            var remoteAlbum3 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.AZW3, new Revision(1)), indexerPriority: 1);

            var decisions = new List<DownloadDecision>();
            decisions.AddRange(new[] { new DownloadDecision(remoteAlbum1), new DownloadDecision(remoteAlbum2), new DownloadDecision(remoteAlbum3) });

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.Should().Be(remoteAlbum3);
            qualifiedReports.Skip(1).First().RemoteBook.Should().Be(remoteAlbum1);
            qualifiedReports.Last().RemoteBook.Should().Be(remoteAlbum2);
        }

        [Test]
        public void ensure_download_decisions_indexer_priority_is_not_perfered_over_quality()
        {
            var remoteAlbum1 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.EPUB, new Revision(1)), indexerPriority: 25);
            var remoteAlbum2 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.AZW3, new Revision(1)), indexerPriority: 50);
            var remoteAlbum3 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.PDF, new Revision(1)), indexerPriority: 1);
            var remoteAlbum4 = GivenRemoteAlbum(new List<Book> { GivenAlbum(1) }, new QualityModel(Quality.AZW3, new Revision(1)), indexerPriority: 25);

            var decisions = new List<DownloadDecision>();
            decisions.AddRange(new[] { new DownloadDecision(remoteAlbum1), new DownloadDecision(remoteAlbum2), new DownloadDecision(remoteAlbum3), new DownloadDecision(remoteAlbum4) });

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteBook.Should().Be(remoteAlbum4);
            qualifiedReports.Skip(1).First().RemoteBook.Should().Be(remoteAlbum2);
            qualifiedReports.Skip(2).First().RemoteBook.Should().Be(remoteAlbum1);
            qualifiedReports.Last().RemoteBook.Should().Be(remoteAlbum3);
        }
    }
}
