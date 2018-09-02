using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class AcceptableSizeSpecificationFixture : CoreTest<AcceptableSizeSpecification>
    {
        private Movie movie;
        private RemoteMovie remoteMovie;
        private QualityDefinition qualityType;

        [SetUp]
        public void Setup()
        {

            movie = Builder<Movie>.CreateNew().Build();

            qualityType = Builder<QualityDefinition>.CreateNew()
                .With(q => q.MinSize = 2)
                .With(q => q.MaxSize = 10)
                .With(q => q.Quality = Quality.SDTV)
                .Build();

            remoteMovie = new RemoteMovie
            {
                Movie = movie,
                Release = new ReleaseInfo(),
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },

            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));



            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.Get(Quality.SDTV)).Returns(qualityType);
        }


        [TestCase(30, 50, false)]
        [TestCase(30, 250, true)]
        [TestCase(30, 500, false)]
        [TestCase(60, 100, false)]
        [TestCase(60, 500, true)]
        [TestCase(60, 1000, false)]
        public void single_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            movie.Runtime = runtime;
            remoteMovie.Movie = movie;
            remoteMovie.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void should_return_true_if_size_is_zero()
        {
            movie.Runtime = 120;
            remoteMovie.Movie = movie;
            remoteMovie.Release.Size = 0;
            qualityType.MinSize = 10;
            qualityType.MaxSize = 20;

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_30_minute()
        {
            movie.Runtime = 30;
            remoteMovie.Movie = movie;
            remoteMovie.Release.Size = 18457280000;
            qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_60_minute()
        {
            movie.Runtime = 60;
            remoteMovie.Movie = movie;
            remoteMovie.Release.Size = 36857280000;
            qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_use_110_minutes_if_runtime_is_0()
        {
            movie.Runtime = 0;
            remoteMovie.Movie = movie;
            remoteMovie.Release.Size = 1095.Megabytes();

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().Be(true);
            remoteMovie.Release.Size = 1105.Megabytes();
            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().Be(false);
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
