﻿using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class AddFixture : CoreTest<PendingReleaseService>
    {
        private DownloadDecision _temporarilyRejected;
        private Movie _movie;
        private Profile _profile;
        private ReleaseInfo _release;
		private ParsedMovieInfo _parsedMovieInfo;
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .Build();
			
            _profile = new Profile
                       {
                           Name = "Test",
                           Cutoff = Quality.HDTV720p,
                           Items = new List<ProfileQualityItem>
                                   {
                                       new ProfileQualityItem { Allowed = true, Quality = Quality.HDTV720p },
                                       new ProfileQualityItem { Allowed = true, Quality = Quality.WEBDL720p },
                                       new ProfileQualityItem { Allowed = true, Quality = Quality.Bluray720p }
                                   },
                       };

            _movie.Profile = new LazyLoaded<Profile>(_profile);

            _release = Builder<ReleaseInfo>.CreateNew().Build();

            _parsedMovieInfo = Builder<ParsedMovieInfo>.CreateNew().Build();
            _parsedMovieInfo.Quality = new QualityModel(Quality.HDTV720p);

            _remoteMovie = new RemoteMovie();
			_remoteMovie.Movie = _movie;
            _remoteMovie.ParsedMovieInfo = _parsedMovieInfo;
            _remoteMovie.Release = _release;
            
            _temporarilyRejected = new DownloadDecision(_remoteMovie, new Rejection("Temp Rejected", RejectionType.Temporary));

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<PendingRelease>());

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_movie);

            Mocker.GetMock<IPrioritizeDownloadDecision>()
                  .Setup(s => s.PrioritizeDecisions(It.IsAny<List<DownloadDecision>>()))
                  .Returns((List<DownloadDecision> d) => d);
        }

        private void GivenHeldRelease(string title, string indexer, DateTime publishDate)
        {
            var release = _release.JsonClone();
            release.Indexer = indexer;
            release.PublishDate = publishDate;


            var heldReleases = Builder<PendingRelease>.CreateListOfSize(1)
                                                   .All()
                                                   .With(h => h.MovieId = _movie.Id)
                                                   .With(h => h.Title = title)
                                                   .With(h => h.Release = release)
                                                   .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(heldReleases);
        }

        [Test]
        public void should_add()
        {
            Subject.Add(_temporarilyRejected);

            VerifyInsert();
        }

        [Test]
        public void should_not_add_if_it_is_the_same_release_from_the_same_indexer()
        {
            GivenHeldRelease(_release.Title, _release.Indexer, _release.PublishDate);

            Subject.Add(_temporarilyRejected);

            VerifyNoInsert();
        }

        [Test]
        public void should_add_if_title_is_different()
        {
            GivenHeldRelease(_release.Title + "-RP", _release.Indexer, _release.PublishDate);

            Subject.Add(_temporarilyRejected);

            VerifyInsert();
        }

        [Test]
        public void should_add_if_indexer_is_different()
        {
            GivenHeldRelease(_release.Title, "AnotherIndexer", _release.PublishDate);

            Subject.Add(_temporarilyRejected);

            VerifyInsert();
        }

        [Test]
        public void should_add_if_publish_date_is_different()
        {
            GivenHeldRelease(_release.Title, _release.Indexer, _release.PublishDate.AddHours(1));

            Subject.Add(_temporarilyRejected);

            VerifyInsert();
        }

        private void VerifyInsert()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                .Verify(v => v.Insert(It.IsAny<PendingRelease>()), Times.Once());
        }

        private void VerifyNoInsert()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                .Verify(v => v.Insert(It.IsAny<PendingRelease>()), Times.Never());
        }
    }
}
