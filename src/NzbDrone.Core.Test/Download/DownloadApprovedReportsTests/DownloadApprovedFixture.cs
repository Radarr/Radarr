using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
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
                .Setup(v => v.PrioritizeDecisions(It.IsAny<List<DownloadDecision>>()))
                .Returns<List<DownloadDecision>>(v => v);
        }

        private Movie GetMovie(int id)
        {
            return Builder<Movie>.CreateNew()
                            .With(e => e.Id = id)
				                 .With(m => m.Tags = new HashSet<int>())

                            .Build();
        }

		private RemoteMovie GetRemoteMovie(QualityModel quality, Movie movie = null)
		{
			if (movie == null)
			{
				movie = GetMovie(1);
			}

			movie.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities(), PreferredTags = new List<string>() };

			var remoteEpisode = new RemoteMovie();
			remoteEpisode.ParsedMovieInfo = new ParsedMovieInfo();
			remoteEpisode.ParsedMovieInfo.Quality = quality;
			remoteEpisode.ParsedMovieInfo.Year = 1998;
			remoteEpisode.ParsedMovieInfo.MovieTitle = "A Movie";
			remoteEpisode.ParsedMovieInfo.MovieTitleInfo = new SeriesTitleInfo();

			remoteEpisode.Movie = movie;

			remoteEpisode.Release = new ReleaseInfo();
			remoteEpisode.Release.PublishDate = DateTime.UtcNow;
			remoteEpisode.Release.Title = "A.Movie.1998";
			remoteEpisode.Release.Size = 200;

			return remoteEpisode;
		}

        [Test]
        public void should_download_report_if_epsiode_was_not_already_downloaded()
        {
            var remoteEpisode = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteMovie>()), Times.Once());
        }

        [Test]
        public void should_only_download_episode_once()
        {
            var remoteEpisode = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));
            decisions.Add(new DownloadDecision(remoteEpisode));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteMovie>()), Times.Once());
        }

        [Test]
        public void should_return_downloaded_reports()
        {
            var remoteEpisode = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(1);
        }

        [Test]
        public void should_return_all_downloaded_reports()
        {
            var remoteEpisode1 = GetRemoteMovie(
													new QualityModel(Quality.HDTV720p),
											GetMovie(1)
												 );

            var remoteEpisode2 = GetRemoteMovie(
													new QualityModel(Quality.HDTV720p),
											GetMovie(2)
												 );

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(2);
        }

        [Test]
        public void should_only_return_downloaded_reports()
        {
            var remoteEpisode1 = GetRemoteMovie(
													new QualityModel(Quality.HDTV720p),
											GetMovie(1)
												 );

            var remoteEpisode2 = GetRemoteMovie(
													new QualityModel(Quality.HDTV720p),
											GetMovie(2)
												 );

            var remoteEpisode3 = GetRemoteMovie(
                                                    new QualityModel(Quality.HDTV720p),
											GetMovie(3)
                                                 );

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));
            decisions.Add(new DownloadDecision(remoteEpisode3));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(2);
        }

        [Test]
        public void should_not_add_to_downloaded_list_when_download_fails()
        {
            var remoteEpisode = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.IsAny<RemoteMovie>())).Throws(new Exception());
            Subject.ProcessDecisions(decisions).Grabbed.Should().BeEmpty();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_an_empty_list_when_none_are_appproved()
        {
            var decisions = new List<DownloadDecision>();
            RemoteEpisode ep = null;
            decisions.Add(new DownloadDecision(ep, new Rejection("Failure!")));
            decisions.Add(new DownloadDecision(ep, new Rejection("Failure!")));

            Subject.GetQualifiedReports(decisions).Should().BeEmpty();
        }

        [Test]
        public void should_not_grab_if_pending()
        {
            var remoteEpisode = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode, new Rejection("Failure!", RejectionType.Temporary)));
            decisions.Add(new DownloadDecision(remoteEpisode));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteMovie>()), Times.Never());
        }

        [Test]
        public void should_not_add_to_pending_if_episode_was_grabbed()
        {
            var remoteEpisode = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));
            decisions.Add(new DownloadDecision(remoteEpisode, new Rejection("Failure!", RejectionType.Temporary)));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IPendingReleaseService>().Verify(v => v.Add(It.IsAny<DownloadDecision>()), Times.Never());
        }

        [Test]
        public void should_add_to_pending_even_if_already_added_to_pending()
        {
           
            var remoteEpisode = GetRemoteMovie(new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode, new Rejection("Failure!", RejectionType.Temporary)));
            decisions.Add(new DownloadDecision(remoteEpisode, new Rejection("Failure!", RejectionType.Temporary)));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IPendingReleaseService>().Verify(v => v.Add(It.IsAny<DownloadDecision>()), Times.Exactly(2));
        }
    }
}
