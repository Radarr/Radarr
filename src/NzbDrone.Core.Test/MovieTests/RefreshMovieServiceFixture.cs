using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    [Ignore("Weird moq errors")]
    public class RefreshMovieServiceFixture : CoreTest<RefreshMovieService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .Build();

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(_movie.Id))
                  .Returns(_movie);

            Mocker.GetMock<IProvideMovieInfo>()
                  .Setup(s => s.GetMovieInfo(It.IsAny<int>(), It.IsAny<Profile>(), false))
                  .Callback<int>(p => { throw new MovieNotFoundException(p.ToString()); });
        }

        private void GivenNewMovieInfo(Movie movie)
        {
            Mocker.GetMock<IProvideMovieInfo>()
                  .Setup(s => s.GetMovieInfo(_movie.ImdbId))
                  .Returns(movie);
        }

        [Test]
        public void should_update_tvrage_id_if_changed()
        {
            var newSeriesInfo = _movie.JsonClone();
            newSeriesInfo.ImdbId = _movie.ImdbId + 1;

            GivenNewMovieInfo(newSeriesInfo);

            Subject.Execute(new RefreshMovieCommand(_movie.Id));

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.UpdateMovie(It.Is<Movie>(s => s.ImdbId == newSeriesInfo.ImdbId)));
        }

        [Test]
        public void should_log_error_if_tmdb_id_not_found()
        {
            Subject.Execute(new RefreshMovieCommand(_movie.Id));

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.UpdateMovie(It.IsAny<Movie>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_tmdb_id_changed()
        {
            var newSeriesInfo = _movie.JsonClone();
            newSeriesInfo.TmdbId = _movie.TmdbId + 1;

            GivenNewMovieInfo(newSeriesInfo);

            Subject.Execute(new RefreshMovieCommand(_movie.Id));

            Mocker.GetMock<IMovieService>()
                .Verify(v => v.UpdateMovie(It.Is<Movie>(s => s.TmdbId == newSeriesInfo.TmdbId)));

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
