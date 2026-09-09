using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class RenameMovieFileServiceFixture : CoreTest<RenameMovieFileService>
    {
        private Movie _movie;
        private List<MovieFile> _movieFiles;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .Build();

            _movieFiles = Builder<MovieFile>.CreateListOfSize(2)
                                                .All()
                                                .With(e => e.MovieId = _movie.Id)
                                                .Build()
                                                .ToList();

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(_movie.Id))
                  .Returns(_movie);

            Mocker.GetMock<INamingConfigService>()
                  .Setup(s => s.GetConfig())
                  .Returns(new NamingConfig { RenameMovies = true });
        }

        private void GivenNoMovieFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetMovies(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<MovieFile>());
        }

        private void GivenMovieFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetMovies(It.IsAny<IEnumerable<int>>()))
                  .Returns(_movieFiles);
        }

        private void GivenMovedFiles()
        {
            Mocker.GetMock<IMoveMovieFiles>()
                  .Setup(s => s.MoveMovieFile(It.IsAny<MovieFile>(), _movie));
        }

        private void GivenMovies()
        {
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovies(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Movie> { _movie });
        }

        private void GivenNoMovieFilesByMovies()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesByMovies(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<MovieFile>());
        }

        private void GivenExpectedMovieFolder(Movie movie, string expectedPath)
        {
            Mocker.GetMock<IMovieFolderService>()
                  .Setup(s => s.GetExpectedMovieFolder(movie, It.IsAny<NamingConfig>()))
                  .Returns(expectedPath);
        }

        [Test]
        public void should_include_movie_folder_preview_when_folder_does_not_match_expected_path()
        {
            var expectedPath = "C:\\Movies\\Correct Name (2020)".AsOsAgnostic();
            _movie.Path = "C:\\Movies\\Wrong Name".AsOsAgnostic();

            GivenMovies();
            GivenNoMovieFilesByMovies();
            GivenExpectedMovieFolder(_movie, expectedPath);

            var result = Subject.GetRenamePreviews(new List<int> { _movie.Id });

            result.Should().ContainSingle(p => p.IsMovieFolder &&
                                                p.MovieFileId == 0 &&
                                                p.ExistingPath == _movie.Path &&
                                                p.NewPath == expectedPath);
        }

        [Test]
        public void should_not_include_movie_folder_preview_when_folder_already_matches_expected_path()
        {
            GivenMovies();
            GivenNoMovieFilesByMovies();
            GivenExpectedMovieFolder(_movie, _movie.Path);

            var result = Subject.GetRenamePreviews(new List<int> { _movie.Id });

            result.Should().NotContain(p => p.IsMovieFolder);
        }

        [Test]
        public void should_not_include_movie_folder_preview_when_movie_folder_service_returns_null()
        {
            _movie.Path = "C:\\Movies\\Wrong Name".AsOsAgnostic();

            GivenMovies();
            GivenNoMovieFilesByMovies();
            GivenExpectedMovieFolder(_movie, null);

            var result = Subject.GetRenamePreviews(new List<int> { _movie.Id });

            result.Should().NotContain(p => p.IsMovieFolder);
        }

        [Test]
        public void should_move_movie_folder_before_renaming_files_when_rename_folder_is_true()
        {
            var sourcePath = "C:\\Movies\\Wrong Name".AsOsAgnostic();
            var expectedPath = "C:\\Movies\\Correct Name (2020)".AsOsAgnostic();
            _movie.Path = sourcePath;

            GivenMovieFiles();
            GivenMovedFiles();
            GivenExpectedMovieFolder(_movie, expectedPath);

            Mocker.GetMock<IMovieFolderService>()
                  .Setup(s => s.TryMoveMovieFolder(_movie, sourcePath, expectedPath))
                  .Returns(true);

            Subject.Execute(new RenameFilesCommand(_movie.Id, new List<int> { 1 }) { RenameFolder = true });

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(_movie, sourcePath, expectedPath), Times.Once());

            Mocker.GetMock<IMoveMovieFiles>()
                  .Verify(v => v.MoveMovieFile(It.IsAny<MovieFile>(), _movie), Times.Exactly(2));
        }

        [Test]
        public void should_not_move_movie_folder_when_rename_folder_is_false()
        {
            _movie.Path = "C:\\Movies\\Wrong Name".AsOsAgnostic();

            GivenMovieFiles();
            GivenMovedFiles();
            GivenExpectedMovieFolder(_movie, "C:\\Movies\\Correct Name (2020)".AsOsAgnostic());

            Subject.Execute(new RenameFilesCommand(_movie.Id, new List<int> { 1 }) { RenameFolder = false });

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(It.IsAny<Movie>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_skip_file_rename_for_movie_when_folder_move_fails()
        {
            _movie.Path = "C:\\Movies\\Wrong Name".AsOsAgnostic();

            GivenMovieFiles();
            GivenMovedFiles();
            GivenExpectedMovieFolder(_movie, "C:\\Movies\\Correct Name (2020)".AsOsAgnostic());

            Mocker.GetMock<IMovieFolderService>()
                  .Setup(s => s.TryMoveMovieFolder(It.IsAny<Movie>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(false);

            Subject.Execute(new RenameFilesCommand(_movie.Id, new List<int> { 1 }) { RenameFolder = true });

            Mocker.GetMock<IMoveMovieFiles>()
                  .Verify(v => v.MoveMovieFile(It.IsAny<MovieFile>(), It.IsAny<Movie>()), Times.Never());
        }

        [Test]
        public void should_correct_folder_for_every_movie_in_bulk_rename_command_and_isolate_failures()
        {
            var otherMovieSourcePath = "C:\\Movies\\Other Wrong Name".AsOsAgnostic();
            var otherMovie = Builder<Movie>.CreateNew()
                                            .With(m => m.Path = otherMovieSourcePath)
                                            .Build();

            var expectedOtherPath = "C:\\Movies\\Other Correct Name (2020)".AsOsAgnostic();
            var expectedMoviePath = "C:\\Movies\\Correct Name (2020)".AsOsAgnostic();
            var movieSourcePath = "C:\\Movies\\Wrong Name".AsOsAgnostic();
            _movie.Path = movieSourcePath;

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovies(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Movie> { _movie, otherMovie });

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesByMovie(It.IsAny<int>()))
                  .Returns(new List<MovieFile>());

            GivenExpectedMovieFolder(_movie, expectedMoviePath);
            GivenExpectedMovieFolder(otherMovie, expectedOtherPath);

            // The first movie's physical folder move fails; the second movie should still be processed.
            Mocker.GetMock<IMovieFolderService>()
                  .Setup(s => s.TryMoveMovieFolder(_movie, movieSourcePath, expectedMoviePath))
                  .Returns(false);

            Mocker.GetMock<IMovieFolderService>()
                  .Setup(s => s.TryMoveMovieFolder(otherMovie, otherMovieSourcePath, expectedOtherPath))
                  .Returns(true);

            Subject.Execute(new RenameMovieCommand { MovieIds = new List<int> { _movie.Id, otherMovie.Id } });

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(_movie, movieSourcePath, expectedMoviePath), Times.Once());

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(otherMovie, otherMovieSourcePath, expectedOtherPath), Times.Once());

            // Only the successfully-moved movie (otherMovie) should have its files fetched for renaming;
            // the movie whose folder move failed (_movie) is skipped entirely. Note: NBuilder may assign
            // both movies the same Id, so this asserts on total call count rather than a specific Id.
            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.GetFilesByMovie(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_not_publish_event_if_no_files_to_rename()
        {
            GivenNoMovieFiles();

            Subject.Execute(new RenameFilesCommand(_movie.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<MovieRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_not_publish_event_if_no_files_are_renamed()
        {
            GivenMovieFiles();

            Mocker.GetMock<IMoveMovieFiles>()
                  .Setup(s => s.MoveMovieFile(It.IsAny<MovieFile>(), It.IsAny<Movie>()))
                  .Throws(new SameFilenameException("Same file name", "Filename"));

            Subject.Execute(new RenameFilesCommand(_movie.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<MovieRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_publish_event_if_files_are_renamed()
        {
            GivenMovieFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_movie.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<MovieRenamedEvent>()), Times.Once());
        }

        [Test]
        public void should_update_moved_files()
        {
            GivenMovieFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_movie.Id, new List<int> { 1 }));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<MovieFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_get_moviefiles_by_ids_only()
        {
            GivenMovieFiles();
            GivenMovedFiles();

            var files = new List<int> { 1 };

            Subject.Execute(new RenameFilesCommand(_movie.Id, files));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.GetMovies(files), Times.Once());
        }
    }
}
