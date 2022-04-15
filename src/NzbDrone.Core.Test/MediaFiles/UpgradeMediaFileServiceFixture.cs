using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class UpgradeMediaFileServiceFixture : CoreTest<UpgradeMediaFileService>
    {
        private MovieFile _movieFile;
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _localMovie = new LocalMovie();
            _localMovie.Movie = new Movie
            {
                Path = @"C:\Test\Movies\Movie".AsOsAgnostic(),
                QualityProfiles = new List<Profile>
                {
                    new Profile
                    {
                        Cutoff = Quality.HDTV720p.Id,
                        Items = Qualities.QualityFixture.GetDefaultQualities(),
                        UpgradeAllowed = true
                    }
                }
            };

            _movieFile = Builder<MovieFile>
                  .CreateNew()
                  .With(f => f.Id = 0)
                  .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderExists(Directory.GetParent(_localMovie.Movie.Path).FullName))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.GetParentFolder(It.IsAny<string>()))
                  .Returns<string>(c => Path.GetDirectoryName(c));

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(c => c.ParseCustomFormat(It.IsAny<MovieFile>()))
                  .Returns(new List<CustomFormat>());
        }

        private void GivenSingleMovieWithSingleMovieFile()
        {
            var movieFile =
                new MovieFile
                {
                    Id = 1,
                    RelativePath = @"A.Movie.2019.avi",
                    Quality = new QualityModel(Quality.HDTV720p)
                };

            _localMovie.Movie.MovieFiles = new List<MovieFile> { movieFile };
        }

        [Test]
        public void should_delete_single_movie_file_once()
        {
            GivenSingleMovieWithSingleMovieFile();

            Subject.UpgradeMovieFile(_movieFile, _localMovie);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_delete_movie_file_from_database()
        {
            GivenSingleMovieWithSingleMovieFile();

            Subject.UpgradeMovieFile(_movieFile, _localMovie);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(It.IsAny<MovieFile>(), DeleteMediaFileReason.Upgrade), Times.Once());
        }

        [Test]
        public void should_delete_existing_file_fromdb_if_file_doesnt_exist()
        {
            GivenSingleMovieWithSingleMovieFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeMovieFile(_movieFile, _localMovie);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localMovie.Movie.MovieFiles.Value.First(), DeleteMediaFileReason.Upgrade), Times.Once());
        }

        [Test]
        public void should_not_try_to_recyclebin_existing_file_if_file_doesnt_exist()
        {
            GivenSingleMovieWithSingleMovieFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeMovieFile(_movieFile, _localMovie);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_old_movie_file_in_oldFiles()
        {
            GivenSingleMovieWithSingleMovieFile();

            Subject.UpgradeMovieFile(_movieFile, _localMovie).OldFiles.Count.Should().Be(1);
        }

        [Test]
        public void should_throw_if_there_are_existing_movie_files_and_the_root_folder_is_missing()
        {
            GivenSingleMovieWithSingleMovieFile();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderExists(Directory.GetParent(_localMovie.Movie.Path).FullName))
                  .Returns(false);

            Assert.Throws<RootFolderNotFoundException>(() => Subject.UpgradeMovieFile(_movieFile, _localMovie));

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localMovie.Movie.MovieFiles.Value.First(), DeleteMediaFileReason.Upgrade), Times.Never());
        }
    }
}
