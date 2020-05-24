using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class RefreshMovieServiceFixture : CoreTest<RefreshMovieService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                .With(s => s.Status = MovieStatusType.Released)
                .Build();

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(_movie.Id))
                  .Returns(_movie);

            Mocker.GetMock<IProvideMovieInfo>()
                  .Setup(s => s.GetMovieInfoAsync(It.IsAny<int>()))
                .ThrowsAsync(new MovieNotFoundException(1));
        }

        private void GivenNewMovieInfo(Movie movie)
        {
            Mocker.GetMock<IProvideMovieInfo>()
                .Setup(s => s.GetMovieInfoAsync(_movie.TmdbId))
                .ReturnsAsync(new Tuple<Movie, List<Credit>>(movie, new List<Credit>()));
        }

        [Test]
        public void should_update_imdb_id_if_changed()
        {
            var newMovieInfo = _movie.JsonClone();
            newMovieInfo.ImdbId = _movie.ImdbId + 1;

            GivenNewMovieInfo(newMovieInfo);

            Subject.Execute(new RefreshMovieCommand(_movie.Id));

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.UpdateMovie(It.Is<List<Movie>>(s => s.First().ImdbId == newMovieInfo.ImdbId), true));
        }

        [Test]
        public void should_log_error_if_tmdb_id_not_found()
        {
            Subject.Execute(new RefreshMovieCommand(_movie.Id));

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.UpdateMovie(It.Is<Movie>(s => s.Status == MovieStatusType.Deleted)), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_tmdb_id_changed()
        {
            var newMovieInfo = _movie.JsonClone();
            newMovieInfo.TmdbId = _movie.TmdbId + 1;

            GivenNewMovieInfo(newMovieInfo);

            Subject.Execute(new RefreshMovieCommand(_movie.Id));

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.UpdateMovie(It.Is<List<Movie>>(s => s.First().TmdbId == newMovieInfo.TmdbId), true));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_mark_as_deleted_if_tmdb_id_not_found()
        {
            Subject.Execute(new RefreshMovieCommand(_movie.Id));

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.UpdateMovie(It.Is<Movie>(s => s.Status == MovieStatusType.Deleted)), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_remark_as_deleted_if_tmdb_id_not_found()
        {
            _movie.Status = MovieStatusType.Deleted;

            Subject.Execute(new RefreshMovieCommand(_movie.Id));

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.UpdateMovie(It.IsAny<Movie>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
