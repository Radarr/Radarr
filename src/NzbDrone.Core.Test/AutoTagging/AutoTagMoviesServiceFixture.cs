using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.AutoTagging
{
    [TestFixture]
    public class AutoTagMoviesServiceFixture : CoreTest<AutoTagMoviesService>
    {
        private List<Movie> _movies;

        [SetUp]
        public void Setup()
        {
            _movies = Builder<Movie>.CreateListOfSize(3)
                .All()
                .With(m => m.Tags = new HashSet<int>())
                .Build()
                .ToList();

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetAllMovies())
                  .Returns(_movies);
        }

        [Test]
        public void should_update_only_movies_with_tag_changes()
        {
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.UpdateTags(_movies[0]))
                  .Returns(true);

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.UpdateTags(_movies[1]))
                  .Returns(false);

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.UpdateTags(_movies[2]))
                  .Returns(true);

            Subject.Execute(new AutoTagMoviesCommand());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(_movies[0]), Times.Once());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(_movies[2]), Times.Once());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(_movies[1]), Times.Never());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.IsAny<List<Movie>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_not_update_movies_when_no_tags_change()
        {
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.UpdateTags(It.IsAny<Movie>()))
                  .Returns(false);

            Subject.Execute(new AutoTagMoviesCommand());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.IsAny<Movie>()), Times.Never());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.IsAny<List<Movie>>(), It.IsAny<bool>()), Times.Never());
        }
    }
}
