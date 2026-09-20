using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class UpdateMovieFileServiceTests : CoreTest<UpdateMovieFileService>
    {
        private MovieFile _movieFile;
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                .CreateNew()
                .With(m => m.Path = @"C:\Test\Movies\Movie".AsOsAgnostic())
                .With(m => m.MovieMetadata = new LazyLoaded<MovieMetadata>(Builder<MovieMetadata>
                    .CreateNew()
                    .With(mm => mm.PhysicalRelease = new DateTime(2025, 1, 15))
                    .With(mm => mm.DigitalRelease = new DateTime(2025, 2, 15))
                    .With(mm => mm.InCinemas = new DateTime(2024, 12, 15))
                    .Build()))
                .Build();

            _movieFile = Builder<MovieFile>
                .CreateNew()
                .With(mf => mf.RelativePath = "Movie.2025.mkv")
                .With(mf => mf.Movie = _movie)
                .Build();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileGetLastWrite(It.IsAny<string>()))
                .Returns(DateTime.Now);

            Mocker.GetMock<IConfigService>()
                .Setup(c => c.FileDate)
                .Returns(FileDateType.None);
        }

        [Test]
        public void should_set_file_date_to_physical_release_when_filedate_is_release()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(c => c.FileDate)
                .Returns(FileDateType.Release);

            Subject.ChangeFileDateForFile(_movieFile, _movie);

            var expectedDate = _movie.MovieMetadata.Value.PhysicalRelease.Value;
            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(It.IsAny<string>(), It.Is<DateTime>(d => d.Date == expectedDate.Date)), Times.Once());
        }

        [Test]
        public void should_set_file_date_to_cinemas_when_filedate_is_cinemas()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(c => c.FileDate)
                .Returns(FileDateType.Cinemas);

            Subject.ChangeFileDateForFile(_movieFile, _movie);

            var expectedDate = _movie.MovieMetadata.Value.InCinemas.Value;
            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(It.IsAny<string>(), It.Is<DateTime>(d => d.Date == expectedDate.Date)), Times.Once());
        }

        [Test]
        public void should_preserve_original_file_date_when_filedate_is_preserve_original()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(c => c.FileDate)
                .Returns(FileDateType.PreserveOriginal);

            var preservedDate = new DateTime(2025, 11, 25);

            Subject.ChangeFileDateForFile(_movieFile, _movie, preservedDate);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(It.IsAny<string>(), It.Is<DateTime>(d => d.Date == preservedDate.Date)), Times.Once());
        }

        [Test]
        public void should_not_set_date_when_filedate_is_preserve_original_and_no_preserved_date()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(c => c.FileDate)
                .Returns(FileDateType.PreserveOriginal);

            Subject.ChangeFileDateForFile(_movieFile, _movie, null);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never());
        }

        [Test]
        public void should_not_set_date_when_filedate_is_none()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(c => c.FileDate)
                .Returns(FileDateType.None);

            Subject.ChangeFileDateForFile(_movieFile, _movie);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never());
        }

        [Test]
        public void should_not_set_date_when_release_date_is_missing()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(c => c.FileDate)
                .Returns(FileDateType.Release);

            _movie.MovieMetadata.Value.PhysicalRelease = null;
            _movie.MovieMetadata.Value.DigitalRelease = null;

            Subject.ChangeFileDateForFile(_movieFile, _movie);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never());
        }

        [Test]
        public void should_not_set_date_when_cinemas_date_is_missing()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(c => c.FileDate)
                .Returns(FileDateType.Cinemas);

            _movie.MovieMetadata.Value.InCinemas = null;

            Subject.ChangeFileDateForFile(_movieFile, _movie);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never());
        }
    }
}
