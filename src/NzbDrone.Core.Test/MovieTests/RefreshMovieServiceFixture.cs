using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class RefreshMovieServiceFixture : CoreTest<RefreshMovieService>
    {
        private MovieMetadata _movie;
        private MovieCollection _movieCollection;
        private Movie _existingMovie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<MovieMetadata>.CreateNew()
                .With(s => s.Status = MovieStatusType.Released)
                .Build();

            _movieCollection = Builder<MovieCollection>.CreateNew()
                .Build();

            _existingMovie = Builder<Movie>.CreateNew()
                .With(s => s.MovieMetadata.Value.Status = MovieStatusType.Released)
                .Build();

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(_movie.Id))
                  .Returns(_existingMovie);

            Mocker.GetMock<IMovieMetadataService>()
                  .Setup(s => s.Get(_movie.Id))
                  .Returns(_movie);

            Mocker.GetMock<IAddMovieCollectionService>()
                  .Setup(v => v.AddMovieCollection(It.IsAny<MovieCollection>()))
                  .Returns(_movieCollection);

            Mocker.GetMock<IProvideMovieInfo>()
                  .Setup(s => s.GetMovieInfo(It.IsAny<int>()))
                  .Callback<int>((i) => { throw new MovieNotFoundException(i); });

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>(), null))
                  .Returns(string.Empty);

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(_existingMovie))
                .Returns(new AutoTaggingChanges());
        }

        private void GivenNewMovieInfo(MovieMetadata movie)
        {
            Mocker.GetMock<IProvideMovieInfo>()
                  .Setup(s => s.GetMovieInfo(_movie.TmdbId))
                  .Returns(new Tuple<MovieMetadata, List<Credit>>(movie, new List<Credit>()));
        }

        [Test]
        public void should_update_imdb_id_if_changed()
        {
            var newMovieInfo = _movie.JsonClone();
            newMovieInfo.ImdbId = _movie.ImdbId + 1;

            GivenNewMovieInfo(newMovieInfo);

            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.Is<MovieMetadata>(s => s.ImdbId == newMovieInfo.ImdbId)));
        }

        [Test]
        public void should_log_error_if_tmdb_id_not_found()
        {
            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.Is<MovieMetadata>(s => s.Status == MovieStatusType.Deleted)), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_tmdb_id_changed()
        {
            var newMovieInfo = _movie.JsonClone();
            newMovieInfo.TmdbId = _movie.TmdbId + 1;

            GivenNewMovieInfo(newMovieInfo);

            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.Is<MovieMetadata>(s => s.TmdbId == newMovieInfo.TmdbId)));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_mark_as_deleted_if_tmdb_id_not_found()
        {
            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.Is<MovieMetadata>(s => s.Status == MovieStatusType.Deleted)), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_remark_as_deleted_if_tmdb_id_not_found()
        {
            _movie.Status = MovieStatusType.Deleted;

            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.IsAny<MovieMetadata>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
