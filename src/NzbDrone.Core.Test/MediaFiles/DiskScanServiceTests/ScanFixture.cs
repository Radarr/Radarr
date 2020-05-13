using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.DiskScanServiceTests
{
    [TestFixture]
    public class ScanFixture : CoreTest<DiskScanService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\Movies\Movie".AsOsAgnostic())
                                     .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns((string path) => Directory.GetParent(path).FullName);
        }

        private void GivenParentFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(It.IsAny<string>()))
                  .Returns(new string[] { @"C:\Test\Movies\Movie2".AsOsAgnostic() });
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

            Mocker.GetMock<IMediaFileTableCleanupService>()
                .Verify(v => v.Clean(It.IsAny<Movie>(), It.IsAny<List<string>>()), Times.Never());
        }

        [Test]
        public void should_not_scan_if_movie_root_folder_is_empty()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(It.IsAny<string>()))
                  .Returns(new string[0]);

            Subject.Scan(_movie);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Movie>(), new List<string>()), Times.Never());
        }

        [Test]
        public void should_not_scan_extras_subfolder()
        {
            GivenParentFolderExists();

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
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie), Times.Once());
        }

        [Test]
        public void should_not_scan_AppleDouble_subfolder()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, ".AppleDouble", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, ".appledouble", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie), Times.Once());
        }

        [Test]
        public void should_scan_extras_movie_and_subfolders()
        {
            GivenParentFolderExists();
            _movie.Path = @"C:\Test\Movies\Extras".AsOsAgnostic();

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
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 4), _movie), Times.Once());
        }

        [Test]
        public void should_not_scan_subfolders_that_start_with_period()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, ".@__THUMB", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, ".hidden", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie), Times.Once());
        }

        [Test]
        public void should_not_scan_subfolder_of_season_folder_that_starts_with_a_period()
        {
            GivenParentFolderExists();

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
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie), Times.Once());
        }

        [Test]
        public void should_not_scan_Synology_eaDir()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "@eaDir", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie), Times.Once());
        }

        [Test]
        public void should_not_scan_thumb_folder()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie), Times.Once());
        }

        [Test]
        public void should_scan_dotHack_folder()
        {
            GivenParentFolderExists();
            _movie.Path = @"C:\Test\TV\.hack".AsOsAgnostic();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "Season 1", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 2), _movie), Times.Once());
        }

        [Test]
        public void should_find_files_at_root_of_series_folder()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 2), _movie), Times.Once());
        }

        [Test]
        public void should_exclude_osx_metadata_files()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_movie.Path, "._24 The Status Quo Combustion.mp4").AsOsAgnostic(),
                           Path.Combine(_movie.Path, "24 The Status Quo Combustion.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_movie);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _movie), Times.Once());
        }
    }
}
