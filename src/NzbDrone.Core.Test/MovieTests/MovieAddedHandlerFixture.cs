using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    public class MovieAddedHandlerFixture : CoreTest<MovieAddedHandler>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                    .With(m => m.Path = "C:\\Movies\\Wrong Name".AsOsAgnostic())
                                    .With(m => m.AddOptions = new AddMovieOptions
                                    {
                                        Monitor = MonitorTypes.MovieOnly,
                                        RenameFolderOnImport = false
                                    })
                                    .Build();

            Mocker.GetMock<INamingConfigService>()
                  .Setup(s => s.GetConfig())
                  .Returns(new NamingConfig { RenameMovies = true });
        }

        private void GivenExpectedMovieFolder(string expectedPath)
        {
            Mocker.GetMock<IMovieFolderService>()
                  .Setup(s => s.GetExpectedMovieFolder(_movie, It.IsAny<NamingConfig>()))
                  .Returns(expectedPath);
        }

        [Test]
        public void should_not_rename_folder_when_add_options_is_null()
        {
            _movie.AddOptions = null;

            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.GetExpectedMovieFolder(It.IsAny<Movie>(), It.IsAny<NamingConfig>()), Times.Never());
        }

        [Test]
        public void should_not_rename_folder_when_rename_folder_on_import_is_false()
        {
            _movie.AddOptions.RenameFolderOnImport = false;

            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(It.IsAny<Movie>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_rename_folder_on_movie_added_event_when_flag_set_and_mismatched()
        {
            var expectedPath = "C:\\Movies\\Correct Name (2020)".AsOsAgnostic();
            _movie.AddOptions.RenameFolderOnImport = true;

            GivenExpectedMovieFolder(expectedPath);

            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(_movie, "C:\\Movies\\Wrong Name".AsOsAgnostic(), expectedPath), Times.Once());
        }

        [Test]
        public void should_rename_folder_for_every_movie_on_movies_imported_event()
        {
            var expectedPath = "C:\\Movies\\Correct Name (2020)".AsOsAgnostic();
            var otherExpectedPath = "C:\\Movies\\Other Correct Name (2021)".AsOsAgnostic();
            var otherMovie = Builder<Movie>.CreateNew()
                                            .With(m => m.Path = "C:\\Movies\\Other Wrong Name".AsOsAgnostic())
                                            .With(m => m.AddOptions = new AddMovieOptions
                                            {
                                                Monitor = MonitorTypes.MovieOnly,
                                                RenameFolderOnImport = true
                                            })
                                            .Build();

            _movie.AddOptions.RenameFolderOnImport = true;

            GivenExpectedMovieFolder(expectedPath);

            Mocker.GetMock<IMovieFolderService>()
                  .Setup(s => s.GetExpectedMovieFolder(otherMovie, It.IsAny<NamingConfig>()))
                  .Returns(otherExpectedPath);

            Subject.Handle(new MoviesImportedEvent(new List<Movie> { _movie, otherMovie }));

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(_movie, "C:\\Movies\\Wrong Name".AsOsAgnostic(), expectedPath), Times.Once());

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(otherMovie, "C:\\Movies\\Other Wrong Name".AsOsAgnostic(), otherExpectedPath), Times.Once());
        }

        [Test]
        public void should_not_rename_when_expected_folder_matches_current_path()
        {
            _movie.AddOptions.RenameFolderOnImport = true;

            GivenExpectedMovieFolder(_movie.Path);

            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IMovieFolderService>()
                  .Verify(v => v.TryMoveMovieFolder(It.IsAny<Movie>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_still_push_refresh_movie_command_on_movie_added_event()
        {
            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<RefreshMovieCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Once());
        }

        [Test]
        public void should_still_push_refresh_movie_commands_on_movies_imported_event()
        {
            Subject.Handle(new MoviesImportedEvent(new List<Movie> { _movie }));

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.PushMany(It.IsAny<List<RefreshMovieCommand>>()), Times.Once());
        }
    }
}
