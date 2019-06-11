using System.Collections.Generic;
using FizzWare.NBuilder;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemoveGrabbedFixture : CoreTest<PendingReleaseService>
    {
        private DownloadDecision _temporarilyRejected;
        private Movie _movie;
        private Profile _profile;
        private ReleaseInfo _release;
        private ParsedMovieInfo _parsedEpisodeInfo;
        private RemoteMovie _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .Build();

            _profile = new Profile
                       {
                           Name = "Test",
                           Cutoff = Quality.HDTV720p.Id,
                           Items = new List<ProfileQualityItem>
                                   {
                                       new ProfileQualityItem { Allowed = true, Quality = Quality.HDTV720p },
                                       new ProfileQualityItem { Allowed = true, Quality = Quality.WEBDL720p },
                                       new ProfileQualityItem { Allowed = true, Quality = Quality.Bluray720p }
                                   },
                       };

            _movie.Profile = new LazyLoaded<Profile>(_profile);

            _release = Builder<ReleaseInfo>.CreateNew().Build();

            _parsedEpisodeInfo = Builder<ParsedMovieInfo>.CreateNew().Build();
            _parsedEpisodeInfo.Quality = new QualityModel(Quality.HDTV720p);

            _remoteEpisode = new RemoteMovie();
            //_remoteEpisode.Episodes = new List<Episode>{ _episode };
            _remoteEpisode.Movie = _movie;
            _remoteEpisode.ParsedMovieInfo = _parsedEpisodeInfo;
            _remoteEpisode.Release = _release;

            _temporarilyRejected = new DownloadDecision(_remoteEpisode, new Rejection("Temp Rejected", RejectionType.Temporary));

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<PendingRelease>());

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_movie);

            //Mocker.GetMock<IParsingService>()
            //      .Setup(s => s.GetMovie(It.IsAny<ParsedMovieInfo>(), _series.Title))
            //      .Returns(_episode);

            Mocker.GetMock<IPrioritizeDownloadDecision>()
                  .Setup(s => s.PrioritizeDecisionsForMovies(It.IsAny<List<DownloadDecision>>()))
                  .Returns((List<DownloadDecision> d) => d);
        }

        private void GivenHeldRelease(QualityModel quality)
        {
            var parsedEpisodeInfo = _parsedEpisodeInfo.JsonClone();
            parsedEpisodeInfo.Quality = quality;

            var heldReleases = Builder<PendingRelease>.CreateListOfSize(1)
                                                   .All()
                                                   .With(h => h.MovieId = _movie.Id)
                                                   .With(h => h.Release = _release.JsonClone())
                                                   .With(h => h.ParsedMovieInfo = parsedEpisodeInfo)
                                                   .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(heldReleases);
        }

        [Test]
        public void should_delete_if_the_grabbed_quality_is_the_same()
        {
            GivenHeldRelease(_parsedEpisodeInfo.Quality);

            Subject.Handle(new MovieGrabbedEvent(_remoteEpisode));

            VerifyDelete();
        }

        [Test]
        public void should_delete_if_the_grabbed_quality_is_the_higher()
        {
            GivenHeldRelease(new QualityModel(Quality.SDTV));

            Subject.Handle(new MovieGrabbedEvent(_remoteEpisode));

            VerifyDelete();
        }

        [Test]
        public void should_not_delete_if_the_grabbed_quality_is_the_lower()
        {
            GivenHeldRelease(new QualityModel(Quality.Bluray720p));

            Subject.Handle(new MovieGrabbedEvent(_remoteEpisode));

            VerifyNoDelete();
        }

        private void VerifyDelete()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                .Verify(v => v.Delete(It.IsAny<PendingRelease>()), Times.Once());
        }

        private void VerifyNoDelete()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                .Verify(v => v.Delete(It.IsAny<PendingRelease>()), Times.Never());
        }
    }
}
