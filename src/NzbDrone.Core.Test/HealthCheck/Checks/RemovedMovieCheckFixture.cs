using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class RemovedMovieCheckFixture : CoreTest<RemovedMovieCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        private void GivenMovie(int amount, int deleted)
        {
            List<Movie> movie;

            if (amount == 0)
            {
                movie = new List<Movie>();
            }
            else if (deleted == 0)
            {
                movie = Builder<Movie>.CreateListOfSize(amount)
                    .All()
                    .With(v => v.MovieMetadata.Value.Status = MovieStatusType.Released)
                    .BuildList();
            }
            else
            {
                movie = Builder<Movie>.CreateListOfSize(amount)
                    .All()
                    .With(v => v.MovieMetadata.Value.Status = MovieStatusType.Released)
                    .Random(deleted)
                    .With(v => v.MovieMetadata.Value.Status = MovieStatusType.Deleted)
                    .BuildList();
            }

            Mocker.GetMock<IMovieService>()
                .Setup(v => v.GetAllMovies())
                .Returns(movie);
        }

        [Test]
        public void should_return_error_if_movie_no_longer_on_tmdb()
        {
            GivenMovie(4, 1);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_if_multiple_movie_no_longer_on_tmdb()
        {
            GivenMovie(4, 2);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_ok_if_all_movie_still_on_tmdb()
        {
            GivenMovie(4, 0);

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_if_no_movie_exist()
        {
            GivenMovie(0, 0);

            Subject.Check().ShouldBeOk();
        }
    }
}
