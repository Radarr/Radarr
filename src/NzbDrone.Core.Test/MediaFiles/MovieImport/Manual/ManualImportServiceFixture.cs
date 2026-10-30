using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation;
using NzbDrone.Core.MediaFiles.MovieImport.Manual;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Manual
{
    [TestFixture]
    public class ManualImportServiceFixture : CoreTest<ManualImportService>
    {
        private Movie _movie;
        private Movie _versionView;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                   .With(m => m.Id = 1)
                                   .With(m => m.Path = @"C:\Movies\Movie (2022)".AsOsAgnostic())
                                   .With(m => m.QualityProfile = null)
                                   .Build();

            _versionView = _movie.ForVersion(new MovieVersion { Id = 7, MovieId = 1, Path = @"D:\Movies 4K\Movie (2022)".AsOsAgnostic() });

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(1))
                  .Returns(_movie);

            Mocker.GetMock<IMovieVersionService>()
                  .Setup(s => s.GetAdditionalVersionViews(_movie))
                  .Returns(new List<Movie> { _versionView });

            Mocker.GetMock<IMovieVersionService>()
                  .Setup(s => s.GetAdditionalVersionIds(1))
                  .Returns(new List<int> { 7 });

            Mocker.GetMock<IMovieVersionService>()
                  .Setup(s => s.GetVersionView(_movie, 7))
                  .Returns(_versionView);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskScanService>()
                  .Setup(s => s.GetVideoFiles(It.IsAny<string>(), true))
                  .Returns(System.Array.Empty<string>());

            Mocker.GetMock<IDiskScanService>()
                  .Setup(s => s.FilterPaths(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), true))
                  .Returns((string basePath, IEnumerable<string> paths, bool filterExtras) => paths.ToList());

            Mocker.GetMock<IAggregationService>()
                  .Setup(s => s.Augment(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>()))
                  .Returns((LocalMovie localMovie, DownloadClientItem item) => localMovie);

            Mocker.GetMock<IImportApprovedMovie>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), It.IsAny<bool>(), It.IsAny<DownloadClientItem>(), It.IsAny<ImportMode>()))
                  .Returns(new List<ImportResult>());
        }

        private void GivenVideoFiles(string folder, params string[] files)
        {
            Mocker.GetMock<IDiskScanService>()
                  .Setup(s => s.GetVideoFiles(folder, true))
                  .Returns(files);
        }

        [Test]
        public void should_list_files_of_every_version_in_their_own_folder()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesByMovie(1))
                  .Returns(new List<MovieFile>
                  {
                      new MovieFile { Id = 1, MovieId = 1, MovieVersionId = 0, RelativePath = "Movie 1080p.mkv" },
                      new MovieFile { Id = 2, MovieId = 1, MovieVersionId = 7, RelativePath = "Movie 2160p.mkv" }
                  });

            var items = Subject.GetMediaFiles(1);

            items.Should().HaveCount(2);
            items.Single(i => i.MovieFileId == 1).MovieVersionId.Should().Be(0);
            items.Single(i => i.MovieFileId == 1).Path.Should().Be(Path.Combine(_movie.Path, "Movie 1080p.mkv"));
            items.Single(i => i.MovieFileId == 2).MovieVersionId.Should().Be(7);
            items.Single(i => i.MovieFileId == 2).Path.Should().Be(Path.Combine(_versionView.Path, "Movie 2160p.mkv"));
        }

        [Test]
        public void should_list_untracked_file_under_the_version_whose_folder_holds_it()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesByMovie(1))
                  .Returns(new List<MovieFile>());

            var file = Path.Combine(_versionView.Path, "Movie 2160p.mkv");
            GivenVideoFiles(_versionView.Path, file);

            var items = Subject.GetMediaFiles(1);

            items.Should().ContainSingle();
            items.Single().Path.Should().Be(file);
            items.Single().MovieVersionId.Should().Be(7);
        }

        [Test]
        public void should_list_untracked_file_once_when_versions_share_a_folder()
        {
            var sharedView = _movie.ForVersion(new MovieVersion { Id = 7, MovieId = 1, Path = _movie.Path });

            Mocker.GetMock<IMovieVersionService>()
                  .Setup(s => s.GetAdditionalVersionViews(_movie))
                  .Returns(new List<Movie> { sharedView });

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesByMovie(1))
                  .Returns(new List<MovieFile>());

            var file = Path.Combine(_movie.Path, "Movie.mkv");
            GivenVideoFiles(_movie.Path, file);

            var items = Subject.GetMediaFiles(1);

            items.Should().ContainSingle();
            items.Single().MovieVersionId.Should().Be(0);
        }

        [Test]
        public void should_import_into_the_chosen_version()
        {
            Subject.Execute(new ManualImportCommand
            {
                Files = new List<ManualImportFile>
                {
                    new ManualImportFile { Path = @"C:\Downloads\Movie.2022.2160p.mkv".AsOsAgnostic(), MovieId = 1, MovieVersionId = 7, Quality = new QualityModel(Quality.Bluray2160p) }
                }
            });

            Mocker.GetMock<IImportApprovedMovie>()
                  .Verify(v => v.Import(It.Is<List<ImportDecision>>(d => d.Single().LocalMovie.Movie.MovieVersionId == 7), true, null, It.IsAny<ImportMode>()), Times.Once());
        }

        [Test]
        public void should_import_into_the_movie_when_version_is_unknown()
        {
            Subject.Execute(new ManualImportCommand
            {
                Files = new List<ManualImportFile>
                {
                    new ManualImportFile { Path = @"C:\Downloads\Movie.2022.1080p.mkv".AsOsAgnostic(), MovieId = 1, MovieVersionId = 99, Quality = new QualityModel(Quality.Bluray1080p) }
                }
            });

            Mocker.GetMock<IImportApprovedMovie>()
                  .Verify(v => v.Import(It.Is<List<ImportDecision>>(d => d.Single().LocalMovie.Movie == _movie), true, null, It.IsAny<ImportMode>()), Times.Once());
        }
    }
}
