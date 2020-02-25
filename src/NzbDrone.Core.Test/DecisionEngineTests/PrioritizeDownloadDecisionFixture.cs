using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
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
        }

        private RemoteMovie GivenRemoteMovie(QualityModel quality, int age = 0, long size = 0, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet)
        {
            var remoteMovie = new RemoteMovie();
            remoteMovie.ParsedMovieInfo = new ParsedMovieInfo();
            remoteMovie.ParsedMovieInfo.MovieTitle = "A Movie";
            remoteMovie.ParsedMovieInfo.Year = 1998;
            remoteMovie.ParsedMovieInfo.Quality = quality;

            remoteMovie.Movie = Builder<Movie>.CreateNew().With(m => m.Profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                PreferredTags = new List<string> { "DTS-HD", "SPARKS" },
                FormatItems = CustomFormatsFixture.GetSampleFormatItems(_customFormat1.Name, _customFormat2.Name),
                MinFormatScore = 0
            })
                .With(m => m.Title = "A Movie").Build();

            remoteMovie.Release = new ReleaseInfo();
            remoteMovie.Release.PublishDate = DateTime.Now.AddDays(-age);
            remoteMovie.Release.Size = size;
            remoteMovie.Release.DownloadProtocol = downloadProtocol;
            remoteMovie.Release.Title = "A Movie 1998";

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
        public void should_put_propers_before_non_propers()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p, new Revision(version: 1)));
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_put_higher_quality_before_lower()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.SDTV));
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.ParsedMovieInfo.Quality.Quality.Should().Be(Quality.HDTV720p);
        }

        [Test]
        public void should_order_by_age_then_largest_rounded_to_200mb()
        {
            var remoteEpisodeSd = GivenRemoteMovie(new QualityModel(Quality.SDTV), size: 100.Megabytes(), age: 1);
            var remoteEpisodeHdSmallOld = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 1200.Megabytes(), age: 1000);
            var remoteEpisodeSmallYoung = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 1250.Megabytes(), age: 10);
            var remoteEpisodeHdLargeYoung = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), size: 3000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisodeSd));
            decisions.Add(new DownloadDecision(remoteEpisodeHdSmallOld));
            decisions.Add(new DownloadDecision(remoteEpisodeSmallYoung));
            decisions.Add(new DownloadDecision(remoteEpisodeHdLargeYoung));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteEpisodeHdLargeYoung);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), age: 10);
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), age: 5);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Should().Be(remoteEpisode2);
        }

        [Test]
        public void should_put_usenet_above_torrent_when_usenet_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);

            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Torrent);
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
        }

        [Test]
        public void should_put_torrent_above_usenet_when_torrent_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Torrent);

            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Torrent);
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
        }

        [Test]
        public void should_prefer_releases_with_more_seeders()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 100;

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode1.Release.Title = "A Movie 1998";
            remoteEpisode2.Release = torrentInfo2;
            remoteEpisode2.Release.Title = "A Movie 1998";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteMovie.Release).Seeders.Should().Be(torrentInfo2.Seeders);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_given_equal_number_of_seeds()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;
            torrentInfo1.Peers = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Peers = 100;

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode1.Release.Title = "A Movie 1998";
            remoteEpisode2.Release = torrentInfo2;
            remoteEpisode2.Release.Title = "A Movie 1998";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteMovie.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_no_seeds()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 0;
            torrentInfo1.Peers = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 0;
            torrentInfo2.Peers = 100;

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode1.Release.Title = "A Movie 1998";
            remoteEpisode2.Release = torrentInfo2;
            remoteEpisode2.Release.Title = "A Movie 1998";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteMovie.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_first_release_if_peers_and_size_are_too_similar()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

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

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode1.Release.Title = "A Movie 1998";
            remoteEpisode2.Release = torrentInfo2;
            remoteEpisode2.Release.Title = "A Movie 1998";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteMovie.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_prefer_first_release_if_age_and_size_are_too_similar()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            remoteEpisode1.Release.PublishDate = DateTime.UtcNow.AddDays(-100);
            remoteEpisode1.Release.Size = 200.Megabytes();

            remoteEpisode2.Release.PublishDate = DateTime.UtcNow.AddDays(-150);
            remoteEpisode2.Release.Size = 250.Megabytes();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.Should().Be(remoteEpisode1.Release);
        }

        [Test]
        public void should_prefer_more_prioritized_words()
        {
            var remoteEpisode1 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GivenRemoteMovie(new QualityModel(Quality.HDTV720p));

            remoteEpisode1.Release.Title += " DTS-HD";
            remoteEpisode2.Release.Title += " DTS-HD SPARKS";

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.Should().Be(remoteEpisode2.Release);
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
            var quality1 = new QualityModel(Quality.Bluray720p);
            var remoteMovie1 = GivenRemoteMovie(quality1);
            remoteMovie1.CustomFormats.Add(_customFormat1);
            remoteMovie1.CustomFormatScore = remoteMovie1.Movie.Profile.CalculateCustomFormatScore(remoteMovie1.CustomFormats);

            var quality2 = new QualityModel(Quality.Bluray720p);
            var remoteMovie2 = GivenRemoteMovie(quality2);
            remoteMovie2.CustomFormats.AddRange(new List<CustomFormat> { _customFormat1, _customFormat2 });
            remoteMovie2.CustomFormatScore = remoteMovie2.Movie.Profile.CalculateCustomFormatScore(remoteMovie2.CustomFormats);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            var qualifiedReports = Subject.PrioritizeDecisionsForMovies(decisions);
            qualifiedReports.First().RemoteMovie.Release.Should().Be(remoteMovie2.Release);
        }
    }
}
