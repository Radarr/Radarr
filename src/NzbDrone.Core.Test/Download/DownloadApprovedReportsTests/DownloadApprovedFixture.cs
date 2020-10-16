using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadApprovedReportsTests
{
    [TestFixture]
    public class DownloadApprovedFixture : CoreTest<ProcessDownloadDecisions>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IPrioritizeDownloadDecision>()
                .Setup(v => v.PrioritizeDecisionsForMovies(It.IsAny<List<DownloadDecision>>()))
                .Returns<List<DownloadDecision>>(v => v);
        }

        private Movie GetMovie(int id)
        {
            return Builder<Movie>.CreateNew()
                            .With(e => e.Id = id)
                                 .With(m => m.Tags = new HashSet<int>())

                            .Build();
        }

        private RemoteMovie GetRemoteMovie(QualityModel quality, Movie movie = null, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet)
        {
            if (movie == null)
            {
                movie = GetMovie(1);
            }

            movie.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() };

            var remoteMovie = new RemoteMovie()
            {
                ParsedMovieInfo = new ParsedMovieInfo()
                {
                    Quality = quality,
                    Year = 1998,
                    MovieTitles = new List<string> { "A Movie" },
                },
                Movie = movie,

                Release = new ReleaseInfo()
                {
                    PublishDate = DateTime.UtcNow,
                    Title = "A.Movie.1998",
                    Size = 200,
                    DownloadProtocol = downloadProtocol
                }
            };

            return remoteMovie;
        }

        [Test]
        public void should_download_report_if_movie_was_not_already_downloaded()
        {
            var remoteMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteMovie>()), Times.Once());
        }

        [Test]
        public void should_only_download_movie_once()
        {
            var remoteMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie));
            decisions.Add(new DownloadDecision(remoteMovie));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteMovie>()), Times.Once());
        }

        [Test]
        public void should_not_download_if_any_movie_was_already_downloaded()
        {
            var remoteMovie1 = GetRemoteMovie(
                                                    new QualityModel(Quality.HDTV720p));

            var remoteMovie2 = GetRemoteMovie(
                                                    new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteMovie>()), Times.Once());
        }

        [Test]
        public void should_return_downloaded_reports()
        {
            var remoteMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(1);
        }

        [Test]
        public void should_return_all_downloaded_reports()
        {
            var remoteMovie1 = GetRemoteMovie(new QualityModel(Quality.HDTV720p), GetMovie(1));

            var remoteMovie2 = GetRemoteMovie(new QualityModel(Quality.HDTV720p), GetMovie(2));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(2);
        }

        [Test]
        public void should_only_return_downloaded_reports()
        {
            var remoteMovie1 = GetRemoteMovie(
                                                    new QualityModel(Quality.HDTV720p),
                                                    GetMovie(1));

            var remoteMovie2 = GetRemoteMovie(
                                                    new QualityModel(Quality.HDTV720p),
                                                    GetMovie(2));

            var remoteMovie3 = GetRemoteMovie(
                                                    new QualityModel(Quality.HDTV720p),
                                                    GetMovie(2));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie1));
            decisions.Add(new DownloadDecision(remoteMovie2));
            decisions.Add(new DownloadDecision(remoteMovie3));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(2);
        }

        [Test]
        public void should_not_add_to_downloaded_list_when_download_fails()
        {
            var remoteMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.IsAny<RemoteMovie>())).Throws(new Exception());
            Subject.ProcessDecisions(decisions).Grabbed.Should().BeEmpty();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_an_empty_list_when_none_are_appproved()
        {
            var decisions = new List<DownloadDecision>();
            RemoteMovie remoteMovie = null;
            decisions.Add(new DownloadDecision(remoteMovie, new Rejection("Failure!")));
            decisions.Add(new DownloadDecision(remoteMovie, new Rejection("Failure!")));

            Subject.GetQualifiedReports(decisions).Should().BeEmpty();
        }

        [Test]
        public void should_not_grab_if_pending()
        {
            var remoteMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie, new Rejection("Failure!", RejectionType.Temporary)));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteMovie>()), Times.Never());
        }

        [Test]
        public void should_not_add_to_pending_if_movie_was_grabbed()
        {
            var removeMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(removeMovie));
            decisions.Add(new DownloadDecision(removeMovie, new Rejection("Failure!", RejectionType.Temporary)));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IPendingReleaseService>().Verify(v => v.AddMany(It.IsAny<List<Tuple<DownloadDecision, PendingReleaseReason>>>()), Times.Never());
        }

        [Test]
        public void should_add_to_pending_even_if_already_added_to_pending()
        {
            var remoteEpisode = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode, new Rejection("Failure!", RejectionType.Temporary)));
            decisions.Add(new DownloadDecision(remoteEpisode, new Rejection("Failure!", RejectionType.Temporary)));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IPendingReleaseService>().Verify(v => v.AddMany(It.IsAny<List<Tuple<DownloadDecision, PendingReleaseReason>>>()), Times.Once());
        }

        [Test]
        public void should_add_to_failed_if_already_failed_for_that_protocol()
        {
            var remoteMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie));
            decisions.Add(new DownloadDecision(remoteMovie));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.IsAny<RemoteMovie>()))
                  .Throws(new DownloadClientUnavailableException("Download client failed"));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteMovie>()), Times.Once());
        }

        [Test]
        public void should_not_add_to_failed_if_failed_for_a_different_protocol()
        {
            var remoteMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p), null, DownloadProtocol.Usenet);
            var remoteMovie2 = GetRemoteMovie(new QualityModel(Quality.HDTV720p), null, DownloadProtocol.Torrent);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie));
            decisions.Add(new DownloadDecision(remoteMovie2));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.Is<RemoteMovie>(r => r.Release.DownloadProtocol == DownloadProtocol.Usenet)))
                  .Throws(new DownloadClientUnavailableException("Download client failed"));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.Is<RemoteMovie>(r => r.Release.DownloadProtocol == DownloadProtocol.Usenet)), Times.Once());
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.Is<RemoteMovie>(r => r.Release.DownloadProtocol == DownloadProtocol.Torrent)), Times.Once());
        }

        [Test]
        public void should_add_to_rejected_if_release_unavailable_on_indexer()
        {
            var remoteMovie = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteMovie));

            Mocker.GetMock<IDownloadService>()
                  .Setup(s => s.DownloadReport(It.IsAny<RemoteMovie>()))
                  .Throws(new ReleaseUnavailableException(remoteMovie.Release, "That 404 Error is not just a Quirk"));

            var result = Subject.ProcessDecisions(decisions);

            result.Grabbed.Should().BeEmpty();
            result.Rejected.Should().NotBeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
