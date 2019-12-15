using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class QueueSpecificationFixture : CoreTest<QueueSpecification>
    {
        private Movie _movie;
        private RemoteMovie _remoteMovie;

        private Movie _otherMovie;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _movie = Builder<Movie>.CreateNew()
                                     .With(e => e.Profile = new Profile {
                                         Items = Qualities.QualityFixture.GetDefaultQualities(),
                                         UpgradeAllowed = true
                                     })
                                     .Build();

            _otherMovie = Builder<Movie>.CreateNew()
                                          .With(s => s.Id = 2)
                                          .Build();

            _remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                   .With(r => r.Movie = _movie)
                                                   .With(r => r.ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD) })
                                                   .Build();
        }

        private void GivenEmptyQueue()
        {
            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(new List<Queue.Queue>());
        }

        private void GivenQueue(IEnumerable<RemoteMovie> remoteMovies)
        {
            var queue = remoteMovies.Select(remoteMovie => new Queue.Queue
            {
                RemoteMovie = remoteMovie
            });

            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(queue.ToList());
        }

        [Test]
        public void should_return_true_when_queue_is_empty()
        {
            GivenEmptyQueue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_movie_doesnt_match()
        {
            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                       .With(r => r.Movie = _otherMovie)
                                                       .Build();

            GivenQueue(new List<RemoteMovie> { remoteMovie });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_lower()
        {
            _movie.Profile.Cutoff = Quality.Bluray1080p.Id;

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.SDTV)
                                                                                       })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteMovie });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_qualities_are_the_same()
        {
            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.DVD)
                                                                                       })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteMovie });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_in_queue_is_better()
        {
            _movie.Profile.Cutoff = Quality.Bluray1080p.Id;

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.HDTV720p)
                                                                                       })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteMovie });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_quality_in_queue_meets_cutoff()
        {
            _movie.Profile.Cutoff = _remoteMovie.ParsedMovieInfo.Quality.Quality.Id;

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p)
                                                      })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteMovie });

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }


        [Test]
        public void should_return_false_when_quality_is_better_and_upgrade_allowed_is_false_for_quality_profile()
        {
            _movie.Profile.Cutoff = Quality.Bluray1080p.Id;
            _movie.Profile.UpgradeAllowed = false;

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                .With(r => r.Movie = _movie)
                .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                {
                    Quality = new QualityModel(Quality.Bluray1080p)
                })
                .Build();

            GivenQueue(new List<RemoteMovie> { remoteMovie });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }
    }
}
