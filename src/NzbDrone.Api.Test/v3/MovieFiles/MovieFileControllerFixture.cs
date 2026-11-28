using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;
using Radarr.Api.V3.MovieFiles;

namespace NzbDrone.Api.Test.v3.MovieFiles;

[TestFixture]
public class MovieFileControllerFixture : TestBase<MovieFileController>
{
    [Test]
    public void bulk_delete_should_pass_each_file_its_own_movie()
    {
        var movies = Builder<Movie>.CreateListOfSize(2)
            .TheFirst(1).With(m => m.Id = 1)
            .TheNext(1).With(m => m.Id = 2)
            .Build()
            .ToList();

        var movieFiles = Builder<MovieFile>.CreateListOfSize(2)
            .TheFirst(1).With(f => f.Id = 10).With(f => f.MovieId = 1)
            .TheNext(1).With(f => f.Id = 11).With(f => f.MovieId = 2)
            .Build()
            .ToList();

        Mocker.GetMock<IMediaFileService>()
              .Setup(s => s.GetMovies(It.IsAny<IEnumerable<int>>()))
              .Returns(movieFiles);

        Mocker.GetMock<IMovieService>()
              .Setup(s => s.GetMovies(It.IsAny<IEnumerable<int>>()))
              .Returns(movies);

        Mocker.GetMock<IMovieService>()
              .Setup(s => s.GetMovie(1))
              .Returns(movies[0]);

        Mocker.GetMock<IMovieService>()
              .Setup(s => s.GetMovie(2))
              .Returns(movies[1]);

        Subject.DeleteMovieFiles(new MovieFileListResource { MovieFileIds = new List<int> { 10, 11 } });

        Mocker.GetMock<IDeleteMediaFiles>()
              .Verify(d => d.DeleteMovieFile(movies[0], movieFiles[0]), Times.Once());

        Mocker.GetMock<IDeleteMediaFiles>()
              .Verify(d => d.DeleteMovieFile(movies[1], movieFiles[1]), Times.Once());
    }
}
