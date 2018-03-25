using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.Qualities;

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
            QualityDefinitionServiceFixture.SetupDefaultDefinitions();
            Mocker.Resolve<QualityUpgradableSpecification>();

            _movie = Builder<Movie>.CreateNew()
                                     .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .Build();

            _otherMovie = Builder<Movie>.CreateNew()
                                          .With(s => s.Id = 2)
                                          .Build();

            _remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                   .With(r => r.Movie = _movie)
                                                   .With(r => r.ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(QualityWrapper.Dynamic.DVD) })
                                                   .Build();
        }

        private void GivenEmptyQueue()
        {
            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(new List<Queue.Queue>());
        }

        private void GivenQueue(IEnumerable<RemoteMovie> remoteEpisodes)
        {
            var queue = remoteEpisodes.Select(remoteEpisode => new Queue.Queue
            {
                RemoteMovie = remoteEpisode
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
        public void should_return_true_when_series_doesnt_match()
        {
            var remoteEpisode = Builder<RemoteMovie>.CreateNew()
                                                       .With(r => r.Movie = _otherMovie)
                                                       .Build();

            GivenQueue(new List<RemoteMovie> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_lower()
        {
            _movie.Profile.Value.Cutoff = QualityWrapper.Dynamic.Bluray1080p;

            var remoteEpisode = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                                                       {
                                                                                           Quality = new QualityModel(QualityWrapper.Dynamic.SDTV)
                                                                                       })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_episode_doesnt_match()
        {
            var remoteEpisode = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                                                       {
                                                                                           Quality = new QualityModel(QualityWrapper.Dynamic.DVD)
                                                                                       })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_qualities_are_the_same()
        {
            var remoteEpisode = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                                                       {
                                                                                           Quality = new QualityModel(QualityWrapper.Dynamic.DVD)
                                                                                       })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_in_queue_is_better()
        {
            _movie.Profile.Value.Cutoff = QualityWrapper.Dynamic.Bluray1080p;

            var remoteEpisode = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                                                       {
                                                                                           Quality = new QualityModel(QualityWrapper.Dynamic.HDTV720p)
                                                                                       })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_quality_in_queue_meets_cutoff()
        {
            _movie.Profile.Value.Cutoff = _remoteMovie.ParsedMovieInfo.Quality.QualityDefinition;

            var remoteEpisode = Builder<RemoteMovie>.CreateNew()
                                                      .With(r => r.Movie = _movie)
                                                      .With(r => r.ParsedMovieInfo = new ParsedMovieInfo
                                                      {
                                                          Quality = new QualityModel(QualityWrapper.Dynamic.HDTV720p)
                                                      })
                                                      .Build();

            GivenQueue(new List<RemoteMovie> { remoteEpisode });

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }
    }
}
