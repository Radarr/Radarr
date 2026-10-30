using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests.MovieVersionTests
{
    [TestFixture]
    public class MovieVersionServiceFixture : CoreTest<MovieVersionService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                   .With(m => m.Id = 5)
                                   .With(m => m.QualityProfileId = 3)
                                   .With(m => m.Path = "/Movies/Movie (2020)")
                                   .With(m => m.MovieFileId = 0)
                                   .Build();
        }

        [Test]
        public void should_create_primary_version_when_movie_is_added()
        {
            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion>());

            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Insert(It.Is<MovieVersion>(m => m.MovieId == 5 && m.QualityProfileId == 3 && m.Path == "/Movies/Movie (2020)")),
                          Times.Once());
        }

        [Test]
        public void should_not_create_version_when_movie_already_has_one()
        {
            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { new MovieVersion { MovieId = _movie.Id } });

            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Insert(It.IsAny<MovieVersion>()), Times.Never());
        }

        [Test]
        public void should_delete_versions_when_movies_are_deleted()
        {
            Subject.HandleAsync(new MoviesDeletedEvent(new List<Movie> { _movie }, false, false));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.DeleteForMovies(It.Is<List<int>>(l => l.Count == 1 && l[0] == 5)), Times.Once());
        }

        [Test]
        public void should_attach_file_to_its_additional_version()
        {
            var version = new MovieVersion { Id = 7, MovieId = _movie.Id, MovieFileId = 0 };
            var movieFile = new MovieFile { Id = 42, MovieId = _movie.Id, MovieVersionId = 7 };

            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.Get(7))
                  .Returns(version);

            Subject.Handle(new MovieFileAddedEvent(movieFile));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Update(It.Is<MovieVersion>(m => m.Id == 7 && m.MovieFileId == 42)), Times.Once());
        }

        [Test]
        public void should_attach_file_of_the_movie_to_its_primary_version()
        {
            var primary = new MovieVersion { Id = 1, MovieId = _movie.Id };
            var additional = new MovieVersion { Id = 7, MovieId = _movie.Id };
            var movieFile = new MovieFile { Id = 42, MovieId = _movie.Id, MovieVersionId = 0 };

            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { additional, primary });

            Subject.Handle(new MovieFileAddedEvent(movieFile));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Update(It.Is<MovieVersion>(m => m.Id == 1 && m.MovieFileId == 42)), Times.Once());
        }

        [Test]
        public void should_detach_deleted_file_from_its_version()
        {
            var version = new MovieVersion { Id = 7, MovieId = _movie.Id, MovieFileId = 42 };
            var movieFile = new MovieFile { Id = 42, MovieId = _movie.Id, MovieVersionId = 7 };

            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { new MovieVersion { Id = 1, MovieId = _movie.Id, MovieFileId = 10 }, version });

            Subject.Handle(new MovieFileDeletedEvent(movieFile, DeleteMediaFileReason.Manual));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.UpdateMany(It.Is<List<MovieVersion>>(l => l.Count == 1 && l[0].Id == 7 && l[0].MovieFileId == 0)), Times.Once());
        }

        [Test]
        public void should_keep_primary_version_in_sync_when_movie_is_edited()
        {
            var primary = new MovieVersion { Id = 1, MovieId = _movie.Id, Path = "/Old/Movie (2020)", QualityProfileId = 1 };

            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { primary });

            Subject.Handle(new MovieEditedEvent(_movie, _movie));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Update(It.Is<MovieVersion>(m => m.Id == 1 && m.Path == _movie.Path && m.QualityProfileId == _movie.QualityProfileId)), Times.Once());
        }

        [Test]
        public void should_return_the_movie_for_a_file_of_an_unknown_version()
        {
            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { new MovieVersion { Id = 1, MovieId = _movie.Id } });

            Subject.GetMovieForFile(_movie, new MovieFile { MovieId = _movie.Id, MovieVersionId = 99 }).Should().BeSameAs(_movie);
        }

        [Test]
        public void should_return_a_view_for_a_file_of_an_additional_version()
        {
            var additional = new MovieVersion { Id = 7, MovieId = _movie.Id, Path = "/Movies 4K/Movie (2020)", QualityProfileId = 2 };

            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { new MovieVersion { Id = 1, MovieId = _movie.Id }, additional });

            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.Get(7))
                  .Returns(additional);

            var view = Subject.GetMovieForFile(_movie, new MovieFile { MovieId = _movie.Id, MovieVersionId = 7 });

            view.MovieVersionId.Should().Be(7);
            view.Path.Should().Be("/Movies 4K/Movie (2020)");
        }

        private void GivenQualityProfile(int id)
        {
            Mocker.GetMock<IQualityProfileRepository>()
                  .Setup(s => s.Get(id))
                  .Returns(new QualityProfile { Id = id });
        }

        private void GivenPrimaryVersion()
        {
            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { new MovieVersion { Id = 1, MovieId = _movie.Id, QualityProfileId = _movie.QualityProfileId, Path = _movie.Path } });
        }

        [Test]
        public void should_add_version_in_the_movie_folder_without_root_folder()
        {
            GivenQualityProfile(4);
            GivenPrimaryVersion();

            Subject.AddVersion(_movie, 4, null);

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Insert(It.Is<MovieVersion>(m => m.MovieId == _movie.Id && m.QualityProfileId == 4 && m.Path == _movie.Path)), Times.Once());
        }

        [Test]
        public void should_add_version_with_movie_folder_name_in_other_root_folder()
        {
            GivenQualityProfile(4);
            GivenPrimaryVersion();

            Subject.AddVersion(_movie, 4, "/Movies 4K".AsOsAgnostic());

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Insert(It.Is<MovieVersion>(m => m.Path == Path.Combine("/Movies 4K".AsOsAgnostic(), "Movie (2020)"))), Times.Once());
        }

        [Test]
        public void should_not_add_the_same_version_twice()
        {
            GivenQualityProfile(3);

            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { new MovieVersion { Id = 1, MovieId = _movie.Id, QualityProfileId = 3, Path = _movie.Path } });

            Assert.Throws<NzbDroneClientException>(() => Subject.AddVersion(_movie, 3, null));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Insert(It.IsAny<MovieVersion>()), Times.Never());
        }

        [Test]
        public void should_not_add_version_with_unknown_quality_profile()
        {
            Assert.Throws<NzbDroneClientException>(() => Subject.AddVersion(_movie, 42, null));
        }

        [Test]
        public void should_not_remove_the_primary_version()
        {
            var primary = new MovieVersion { Id = 1, MovieId = _movie.Id };

            Mocker.GetMock<IMovieVersionRepository>().Setup(s => s.Get(1)).Returns(primary);
            Mocker.GetMock<IMovieVersionRepository>().Setup(s => s.FindByMovieId(_movie.Id)).Returns(new List<MovieVersion> { primary });

            Assert.Throws<NzbDroneClientException>(() => Subject.RemoveVersion(1));

            Mocker.GetMock<IMovieVersionRepository>().Verify(v => v.Delete(It.IsAny<MovieVersion>()), Times.Never());
        }

        [Test]
        public void should_not_remove_a_version_that_still_has_a_file()
        {
            var primary = new MovieVersion { Id = 1, MovieId = _movie.Id };
            var additional = new MovieVersion { Id = 7, MovieId = _movie.Id, MovieFileId = 42 };

            Mocker.GetMock<IMovieVersionRepository>().Setup(s => s.Get(7)).Returns(additional);
            Mocker.GetMock<IMovieVersionRepository>().Setup(s => s.FindByMovieId(_movie.Id)).Returns(new List<MovieVersion> { primary, additional });

            Assert.Throws<NzbDroneClientException>(() => Subject.RemoveVersion(7));

            Mocker.GetMock<IMovieVersionRepository>().Verify(v => v.Delete(It.IsAny<MovieVersion>()), Times.Never());
        }

        [Test]
        public void should_remove_an_additional_version_without_file()
        {
            var primary = new MovieVersion { Id = 1, MovieId = _movie.Id };
            var additional = new MovieVersion { Id = 7, MovieId = _movie.Id };

            Mocker.GetMock<IMovieVersionRepository>().Setup(s => s.Get(7)).Returns(additional);
            Mocker.GetMock<IMovieVersionRepository>().Setup(s => s.FindByMovieId(_movie.Id)).Returns(new List<MovieVersion> { primary, additional });

            Subject.RemoveVersion(7);

            Mocker.GetMock<IMovieVersionRepository>().Verify(v => v.Delete(additional), Times.Once());
        }
    }
}
