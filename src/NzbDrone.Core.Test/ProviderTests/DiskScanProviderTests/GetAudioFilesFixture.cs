using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using System.IO.Abstractions;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    public class GetAudioFilesFixture : CoreTest<DiskScanService>
    {
        private string[] _fileNames;
        private readonly string path = @"C:\Test\".AsOsAgnostic();

        [SetUp]
        public void Setup()
        {
            _fileNames = new[]
                        {
                            @"30 Rock1.mp3",
                            @"30 Rock2.flac",
                            @"30 Rock3.ogg",
                            @"30 Rock4.m4a",
                            @"30 Rock.avi",
                            @"movie.exe",
                            @"movie"
                        };

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFileInfos(It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(new List<IFileInfo>());
        }

        private IEnumerable<string> GetFiles(string folder, string subFolder = "")
        {
            return _fileNames.Select(f => Path.Combine(folder, subFolder, f));
        }

        private void GivenFiles(IEnumerable<string> files)
        {
            var filesToReturn = files.Select(x => (FileInfoBase)new FileInfo(x)).ToList<IFileInfo>();

            foreach (var file in filesToReturn)
            {
                TestLogger.Debug(file.Name);
            }
            
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFileInfos(It.IsAny<string>(), SearchOption.AllDirectories))
                  .Returns(filesToReturn);
        }

        [Test]
        public void should_check_all_directories()
        {
            Subject.GetAudioFiles(path);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(path, SearchOption.AllDirectories), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(path, SearchOption.TopDirectoryOnly), Times.Never());
        }

        [Test]
        public void should_check_all_directories_when_allDirectories_is_true()
        {
            Subject.GetAudioFiles(path, true);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(path, SearchOption.AllDirectories), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(path, SearchOption.TopDirectoryOnly), Times.Never());
        }

        [Test]
        public void should_check_top_level_directory_only_when_allDirectories_is_false()
        {
            Subject.GetAudioFiles(path, false);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(path, SearchOption.AllDirectories), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(path, SearchOption.TopDirectoryOnly), Times.Once());
        }

        [Test]
        public void should_return_audio_files_only()
        {
            GivenFiles(GetFiles(path));

            Subject.GetAudioFiles(path).Should().HaveCount(4);
        }

        [TestCase("Extras")]
        [TestCase("@eadir")]
        [TestCase("extrafanart")]
        [TestCase("Plex Versions")]
        [TestCase(".secret")]
        [TestCase(".hidden")]
        [TestCase(".unwanted")]
        public void should_filter_certain_sub_folders(string subFolder)
        {
            var files = GetFiles(path).ToList();
            var specialFiles = GetFiles(path, subFolder).ToList();
            var allFiles = files.Concat(specialFiles);

            var filteredFiles = Subject.FilterFiles(path, allFiles);
            filteredFiles.Should().NotContain(specialFiles);
            filteredFiles.Count.Should().BeGreaterThan(0);
        }
    }
}
