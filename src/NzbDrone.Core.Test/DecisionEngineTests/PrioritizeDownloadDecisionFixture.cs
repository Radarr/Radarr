using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    //TODO: Update for custom qualities!
    public class PrioritizeDownloadDecisionFixture : CoreTest<DownloadDecisionPriorizationService>
    {
        private CustomFormat _customFormat1;
        private CustomFormat _customFormat2;

        [SetUp]
        public void Setup()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);

            _customFormat1 = new CustomFormat("My Format 1", new LanguageSpecification { Value = (int)Language.English }) { Id = 1 };
            _customFormat2 = new CustomFormat("My Format 2", new LanguageSpecification { Value = (int)Language.French }) { Id = 2 };

            CustomFormatsFixture.GivenCustomFormats(_customFormat1, _customFormat2);

            Mocker.GetMock<IQualityDefinitionService>()
                  .Setup(s => s.Get(It.IsAny<Quality>()))
                  .Returns(new QualityDefinition { PreferredSize = null });
        }

        private void GivenPreferredSize(double? size)
        {
            Mocker.GetMock<IQualityDefinitionService>()
                  .Setup(s => s.Get(It.IsAny<Quality>()))
                  .Returns(new QualityDefinition { PreferredSize = size });
        }

        private RemoteMovie GivenRemoteMovie(QualityModel quality, int age = 0, long size = 0, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet, int runtime = 150, int indexerPriority = 25)
        {
            var remoteMovie = new RemoteMovie();
            remoteMovie.ParsedMovieInfo = new ParsedMovieInfo();
            remoteMovie.ParsedMovieInfo.MovieTitles = new List<string> { "A Movie" };
            remoteMovie.ParsedMovieInfo.Year = 1998;
            remoteMovie.ParsedMovieInfo.Quality = quality;

            remoteMovie.Movie = Builder<Movie>.CreateNew().With(m => m.Profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                FormatItems = CustomFormatsFixture.GetSampleFormatItems(_customFormat1.Name, _customFormat2.Name),
                MinFormatScore = 0
            })
                .With(m => m.Title = "A Movie")
                .With(m => m.MovieMetadata.Value.Runtime = runtime).Build();

            remoteMovie.Release = new ReleaseInfo();
            remoteMovie.Release.PublishDate = DateTime.Now.AddDays(-age);
            remoteMovie.Release.Size = size;
            remoteMovie.Release.DownloadProtocol = downloadProtocol;
            remoteMovie.Release.Title = "A Movie 1998";
            remoteMovie.Release.IndexerPriority = indexerPriority;

            remoteMovie.CustomFormats = new List<CustomFormat>();
            remoteMovie.CustomFormatScore = 0;

            return remoteMovie;
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
        public void should_put_reals_before_non_reals()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p, new Revision(version: 1, real: 0)));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p, new Revision(version: 1, real: 1)));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Revision.Real.Should().Be(1);
        }

        [Test]
        public void should_put_propers_before_non_propers()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p, new Revision(version: 1)));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_put_higher_quality_before_lower()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.SDTV));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Quality.Should().Be(Quality.HDTV720p);
        }

        [Test]
        public void should_order_by_age_then_largest_rounded_to_200mb()
        {
            var remoteMovieSd = GivenRemoteMovie(new QualityModel(Quality.SDTV), size: 100.Megabytes(), age: 1);
            var remoteMovieHdSmallOld = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 1200.Megabytes(), age: 1000);
            var remoteMovieSmallYoung = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 1250.Megabytes(), age: 10);
            var remoteMovieHdLargeYoung = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 3000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovieSd));
            decisions.Add(new DownloadDecision(remoteMovieHdSmallOld));
            decisions.Add(new DownloadDecision(remoteMovieSmallYoung));
            decisions.Add(new DownloadDecision(remoteMovieHdLargeYoung));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteMovieHdLargeYoung);
        }

        [Test]
        public void should_order_by_closest_to_preferred_size_if_both_over()
        {
            // 2 MB/Min * 150 Min Runtime = 300 MB
            GivenPreferredSize(2);

            var remoteMovieSmall = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 400.Megabytes(), age: 1);
            var remoteMovieLarge = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 15000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovieSmall));
            decisions.Add(new DownloadDecision(remoteMovieLarge));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteMovieSmall);
        }

        [Test]
        public void should_order_by_largest_to_if_zero_runtime()
        {
            // 2 MB/Min * 150 Min Runtime = 300 MB
            GivenPreferredSize(2);

            var remoteMovieSmall = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 400.Megabytes(), age: 1, runtime: 0);
            var remoteMovieLarge = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 15000.Megabytes(), age: 1, runtime: 0);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovieSmall));
            decisions.Add(new DownloadDecision(remoteMovieLarge));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteMovieLarge);
        }

        [Test]
        public void should_order_by_closest_to_preferred_size_if_both_under()
        {
            // 390 MB/Min * 150 Min Runtime = 58,500 MB
            GivenPreferredSize(390);

            var remoteMovieSmall = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 100.Megabytes(), age: 1);
            var remoteMovieLarge = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 15000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovieSmall));
            decisions.Add(new DownloadDecision(remoteMovieLarge));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteMovieLarge);
        }

        [Test]
        public void should_order_by_closest_to_preferred_size_if_preferred_is_in_between()
        {
            // 46 MB/Min * 150 Min Runtime = 6900 MB
            GivenPreferredSize(46);

            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 100.Megabytes(), age: 1);
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 500.Megabytes(), age: 1);
            var remoteMovie3 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 7000.Megabytes(), age: 1);
            var remoteMovie4 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 15000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));
            decisions.Add(new DownloadDecision(remoteMovie3));
            decisions.Add(new DownloadDecision(remoteMovie4));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteMovie3);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), age: 10);
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), age: 5);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteMovie2);
        }

        [Test]
        public void should_put_usenet_above_torrent_when_usenet_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);

            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Torrent);
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
        }

        [Test]
        public void should_put_torrent_above_usenet_when_torrent_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Torrent);

            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Torrent);
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
        }

        [Test]
        public void should_prefer_releases_with_more_seeders()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 100;

            remoteMovie1.Release = torrentInfo1;
            remoteMovie1.Release.Title = "A Movie 1998";
            remoteMovie2.Release = torrentInfo2;
            remoteMovie2.Release.Title = "A Movie 1998";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteMovie.Release).Seeders.Should().Be(torrentInfo2.Seeders);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_given_equal_number_of_seeds()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;
            torrentInfo1.Peers = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Peers = 100;

            remoteMovie1.Release = torrentInfo1;
            remoteMovie1.Release.Title = "A Movie 1998";
            remoteMovie2.Release = torrentInfo2;
            remoteMovie2.Release.Title = "A Movie 1998";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteMovie.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_no_seeds()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 0;
            torrentInfo1.Peers = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 0;
            torrentInfo2.Peers = 100;

            remoteMovie1.Release = torrentInfo1;
            remoteMovie1.Release.Title = "A Movie 1998";
            remoteMovie2.Release = torrentInfo2;
            remoteMovie2.Release.Title = "A Movie 1998";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteMovie.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_first_release_if_peers_and_size_are_too_similar()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

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

            remoteMovie1.Release = torrentInfo1;
            remoteMovie1.Release.Title = "A Movie 1998";
            remoteMovie2.Release = torrentInfo2;
            remoteMovie2.Release.Title = "A Movie 1998";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteMovie.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_prefer_first_release_if_age_and_size_are_too_similar()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            remoteMovie1.Release.PublishDate = DateTime.UtcNow.AddDays(-100);
            remoteMovie1.Release.Size = 200.Megabytes();

            remoteMovie2.Release.PublishDate = DateTime.UtcNow.AddDays(-150);
            remoteMovie2.Release.Size = 250.Megabytes();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.Should().Be(remoteMovie1.Release);
        }

        [Test]
        public void should_prefer_better_custom_format()
        {
            var quality1 = new QualityModel(Quality.Bluray720p);
            var remoteMovie1 = GivenRemoteMovie(quality1);

            var quality2 = new QualityModel(Quality.Bluray720p);
            var remoteMovie2 = GivenRemoteMovie(quality2);
            remoteMovie2.CustomFormats.Add(_customFormat1);
            remoteMovie2.CustomFormatScore = remoteMovie2.Movie.Profile.CalculateCustomFormatScore(remoteMovie2.CustomFormats);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.Should().Be(remoteMovie2.Release);
        }

        [Test]
        public void should_prefer_better_custom_format2()
        {
            var quality1 = new QualityModel(Quality.Bluray720p);
            var remoteMovie1 = GivenRemoteMovie(quality1);
            remoteMovie1.CustomFormats.Add(_customFormat1);
            remoteMovie1.CustomFormatScore = remoteMovie1.Movie.Profile.CalculateCustomFormatScore(remoteMovie1.CustomFormats);

            var quality2 = new QualityModel(Quality.Bluray720p);
            var remoteMovie2 = GivenRemoteMovie(quality2);
            remoteMovie2.CustomFormats.Add(_customFormat2);
            remoteMovie2.CustomFormatScore = remoteMovie2.Movie.Profile.CalculateCustomFormatScore(remoteMovie2.CustomFormats);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.Should().Be(remoteMovie2.Release);
        }

        [Test]
        public void should_prefer_2_custom_formats()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.Bluray720p));
            remoteMovie1.CustomFormats.Add(_customFormat1);
            remoteMovie1.CustomFormatScore = remoteMovie1.Movie.Profile.CalculateCustomFormatScore(remoteMovie1.CustomFormats);

            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.Bluray720p));
            remoteMovie2.CustomFormats.AddRange(new List<CustomFormat> { _customFormat1, _customFormat2 });
            remoteMovie2.CustomFormatScore = remoteMovie2.Movie.Profile.CalculateCustomFormatScore(remoteMovie2.CustomFormats);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.Should().Be(remoteMovie2.Release);
        }

        [Test]
        public void should_prefer_proper_over_score_when_download_propers_is_prefer_and_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p, new Revision(1)));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p, new Revision(2)));

            remoteMovie1.CustomFormatScore = 10;
            remoteMovie2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_prefer_proper_over_score_when_download_propers_is_do_not_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotUpgrade);

            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p, new Revision(1)));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p, new Revision(2)));

            remoteMovie1.CustomFormatScore = 10;
            remoteMovie2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_prefer_score_over_proper_when_download_propers_is_do_not_prefer()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p, new Revision(1)));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p, new Revision(2)));

            remoteMovie1.CustomFormatScore = 10;
            remoteMovie2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Quality.Should().Be(Quality.WEBDL1080p);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Revision.Version.Should().Be(1);
            qualifiedReports.First().RemoteMovie.CustomFormatScore.Should().Be(10);
        }

        [Test]
        public void should_prefer_score_over_real_when_download_propers_is_do_not_prefer()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p, new Revision(1, 0)));
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p, new Revision(1, 1)));

            remoteMovie1.CustomFormatScore = 10;
            remoteMovie2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Quality.Should().Be(Quality.WEBDL1080p);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Revision.Version.Should().Be(1);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Revision.Real.Should().Be(0);
            qualifiedReports.First().RemoteMovie.CustomFormatScore.Should().Be(10);
        }

        [Test]
        public void sort_download_decisions_based_on_indexer_priority()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p), indexerPriority: 25);
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p), indexerPriority: 50);
            var remoteMovie3 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p), indexerPriority: 1);

            var decisions = new List<DownloadDecision>();
            decisions.AddRange(new[] { new DownloadDecision(remoteMovie1), new DownloadDecision(remoteMovie2), new DownloadDecision(remoteMovie3) });

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteMovie3);
            qualifiedReports.Skip(1).First().RemoteMovie.Should().Be(remoteMovie1);
            qualifiedReports.Last().RemoteMovie.Should().Be(remoteMovie2);
        }

        [Test]
        public void ensure_download_decisions_indexer_priority_is_not_perfered_over_quality()
        {
            var remoteMovie1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), indexerPriority: 25);
            var remoteMovie2 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p), indexerPriority: 50);
            var remoteMovie3 = GivenRemoteMovie(new QualityModel(Quality.SDTV), indexerPriority: 1);
            var remoteMovie4 = GivenRemoteMovie(new QualityModel(Quality.WEBDL1080p), indexerPriority: 25);

            var decisions = new List<DownloadDecision>();
            decisions.AddRange(new[] { new DownloadDecision(remoteMovie1), new DownloadDecision(remoteMovie2), new DownloadDecision(remoteMovie3), new DownloadDecision(remoteMovie4) });

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteMovie4);
            qualifiedReports.Skip(1).First().RemoteMovie.Should().Be(remoteMovie2);
            qualifiedReports.Skip(2).First().RemoteMovie.Should().Be(remoteMovie1);
            qualifiedReports.Last().RemoteMovie.Should().Be(remoteMovie3);
        }
    }
}
