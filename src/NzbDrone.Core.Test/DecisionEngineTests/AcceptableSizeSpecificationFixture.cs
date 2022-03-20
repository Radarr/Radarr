using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class AcceptableSizeSpecificationFixture : CoreTest<AcceptableSizeSpecification>
    {
        private Movie _movie;
        private RemoteMovie _remoteMovie;
        private QualityDefinition _qualityType;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew().Build();

            _qualityType = Builder<QualityDefinition>.CreateNew()
                .With(q => q.MinSize = 2)
                .With(q => q.MaxSize = 10)
                .With(q => q.Quality = Quality.SDTV)
                .Build();

            _remoteMovie = new RemoteMovie
            {
                Movie = _movie,
                Release = new ReleaseInfo(),
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.Get(Quality.SDTV)).Returns(_qualityType);
        }

        [TestCase(30, 50, false)]
        [TestCase(30, 250, true)]
        [TestCase(30, 500, false)]
        [TestCase(60, 100, false)]
        [TestCase(60, 500, true)]
        [TestCase(60, 1000, false)]
        public void single_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _movie.MovieMetadata.Value.Runtime = runtime;
            _remoteMovie.Movie = _movie;
            _remoteMovie.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void should_return_true_if_size_is_zero()
        {
            _movie.MovieMetadata.Value.Runtime = 120;
            _remoteMovie.Movie = _movie;
            _remoteMovie.Release.Size = 0;
            _qualityType.MinSize = 10;
            _qualityType.MaxSize = 20;

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_30_minute()
        {
            _movie.MovieMetadata.Value.Runtime = 30;
            _remoteMovie.Movie = _movie;
            _remoteMovie.Release.Size = 18457280000;
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_60_minute()
        {
            _movie.MovieMetadata.Value.Runtime = 60;
            _remoteMovie.Movie = _movie;
            _remoteMovie.Release.Size = 36857280000;
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_use_110_minutes_if_runtime_is_0()
        {
            _movie.MovieMetadata.Value.Runtime = 0;
            _remoteMovie.Movie = _movie;
            _remoteMovie.Release.Size = 1095.Megabytes();

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().Be(true);
            _remoteMovie.Release.Size = 1105.Megabytes();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().Be(false);
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
