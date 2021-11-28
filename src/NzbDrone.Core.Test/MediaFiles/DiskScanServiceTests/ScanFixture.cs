using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Movies;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.DiskScanServiceTests
{
    [TestFixture]
    public class ScanFixture : CoreTest<DiskScanService>
    {
        private Movie _movie;
        private string _rootFolder;
        private string _otherMovieFolder;

        [SetUp]
        public void Setup()
        {
            _rootFolder = @"C:\Test\Movies".AsOsAgnostic();
            _otherMovieFolder = @"C:\Test\Movies\OtherMovie".AsOsAgnostic();
            var movieFolder = @"C:\Test\Movies\Movie".AsOsAgnostic();

            _movie = Builder<Movie>.CreateNew()
                .With(s => s.Path = movieFolder)
                                     .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns((string path) => Directory.GetParent(path).FullName);

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                  .Returns(_rootFolder);

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesByMovie(It.IsAny<int>()))
                  .Returns(new List<MovieFile>());
        }

        private void GivenRootFolder(params string[] subfolders)
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FolderExists(_rootFolder))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(_rootFolder))
                  .Returns(subfolders);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderEmpty(_rootFolder))
                  .Returns(subfolders.Empty());

            foreach (var folder in subfolders)
            {
                Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(folder))
                  .Returns(true);
            }
        }

        private void GivenMovieFolder()
        {
            GivenRootFolder(_movie.Path);
        }

        private void GivenFiles(IEnumerable<string> files)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(It.IsAny<string>(), SearchOption.AllDirectories))
                  .Returns(files.ToArray());
        }

        [Test]
        public void should_not_scan_if_movie_root_folder_does_not_exist()
        {
            Subject.Scan(_movie);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetFiles(_movie.Path, SearchOption.AllDirectories), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.CreateFolder(_movie.Path), Times.Never());

            Mocker.GetMock<IMediaFileTableCleanupService>()
                .Verify(v => v.Clean(It.IsAny<Movie>(), It.IsAny<List<string>>()), Times.Never());
        }

        [Test]
        public void should_not_scan_if_movie_root_folder_is_empty()
        {
            GivenRootFolder();

            Subject.Scan(_movie);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetFiles(_movie.Path, SearchOption.AllDirectories), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.CreateFolder(_movie.Path), Times.Never());

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Movie>(), It.IsAny<List<string>>()), Times.Never());

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.IsAny<List<string>>(), _movie, false), Times.Never());
        }

        [Test]
        public void should_create_if_movie_folder_does_not_exist_but_create_folder_enabled()
        {
            GivenRootFolder(_otherMovieFolder);

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.CreateEmptyMovieFolders)
                  .Returns(true);

            Subject.Scan(_movie);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.CreateFolder(_movie.Path), Times.Once());
        }

        [Test]
        public void should_not_scan_extras_subfolder()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "EXTRAS", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Extras", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "EXTRAs", "file3.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "ExTrAs", "file4.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_not_scan_various_extras_subfolders()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "Behind the Scenes", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Deleted Scenes", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Featurettes", "file3.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Interviews", "file4.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Sample", "file5.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Samples", "file6.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Scenes", "file7.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Shorts", "file8.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Trailers", "file9.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "The Count of Monte Cristo (2002) (1080p BluRay x265 10bit Tigole).mkv").AsOsAgnostic(),
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_not_scan_featurettes_subfolders()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "Featurettes", "An Epic Reborn.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Featurettes", "Deleted & Alternate Scenes.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Featurettes", "En Garde - Multi-Angle Dailies.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Featurettes", "Layer-By-Layer - Sound Design - Multiple Audio.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "The Count of Monte Cristo (2002) (1080p BluRay x265 10bit Tigole).mkv").AsOsAgnostic(),
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_not_create_if_movie_folder_does_not_exist_and_create_folder_disabled()
        {
            GivenRootFolder(_otherMovieFolder);

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.CreateEmptyMovieFolders)
                  .Returns(false);

            Subject.Scan(_movie);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.CreateFolder(_movie.Path), Times.Never());
        }

        [Test]
        public void should_clean_but_not_import_if_movie_folder_does_not_exist()
        {
            GivenRootFolder(_otherMovieFolder);

            Subject.Scan(_movie);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.FolderExists(_movie.Path), Times.Once());

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Movie>(), It.IsAny<List<string>>()), Times.Once());

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.IsAny<List<string>>(), _movie, false), Times.Never());
        }

        [Test]
        public void should_not_scan_AppleDouble_subfolder()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, ".AppleDouble", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, ".appledouble", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_scan_extras_movie_and_subfolders()
        {
            _movie.Path = @"C:\Test\Movies\Extras".AsOsAgnostic();

            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "Extras", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, ".AppleDouble", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e02.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 2", "s02e01.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 2", "s02e02.mkv").AsOsAgnostic(),
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 4), _movie, false), Times.Once());
        }

        [Test]
        public void should_not_scan_subfolders_that_start_with_period()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, ".@__THUMB", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, ".hidden", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_not_scan_subfolder_of_season_folder_that_starts_with_a_period()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "Season 1", ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", ".@__THUMB", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", ".hidden", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", ".AppleDouble", "s01e01.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_not_scan_Synology_eaDir()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "@eaDir", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_not_scan_thumb_folder()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_scan_dotHack_folder()
        {
            _movie.Path = @"C:\Test\TV\.hack".AsOsAgnostic();

            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "Season 1", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 2), _movie, false), Times.Once());
        }

        [Test]
        public void should_find_files_at_root_of_movie_folder()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 2), _movie, false), Times.Once());
        }

        [Test]
        public void should_exclude_inline_extra_files()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "Avatar (2009).mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Deleted Scenes-deleted.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "The World of Pandora-other.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }

        [Test]
        public void should_exclude_osx_metadata_files()
        {
            GivenMovieFolder();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "._24 The Status Quo Combustion.mp4").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "24 The Status Quo Combustion.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie, false), Times.Once());
        }
    }
}
