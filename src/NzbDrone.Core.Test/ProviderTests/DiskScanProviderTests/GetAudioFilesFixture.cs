using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    public class GetAudioFilesFixture : CoreTest<DiskScanService>
    {
        private readonly string _path = @"C:\Test\".AsOsAgnostic();
        private string[] _fileNames;

        [SetUp]
        public void Setup()
        {
            _fileNames = new[]
                        {
                            @"30 Rock1.mp3",
                            @"30 Rock2.flac",
                            @"30 Rock3.pdf",
                            @"30 Rock4.epub",
                            @"30 Rock.mobi",
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
            Subject.GetAudioFiles(_path);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(_path, SearchOption.AllDirectories), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(_path, SearchOption.TopDirectoryOnly), Times.Never());
        }

        [Test]
        public void should_check_all_directories_when_allDirectories_is_true()
        {
            Subject.GetAudioFiles(_path, true);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(_path, SearchOption.AllDirectories), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(_path, SearchOption.TopDirectoryOnly), Times.Never());
        }

        [Test]
        public void should_check_top_level_directory_only_when_allDirectories_is_false()
        {
            Subject.GetAudioFiles(_path, false);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(_path, SearchOption.AllDirectories), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFileInfos(_path, SearchOption.TopDirectoryOnly), Times.Once());
        }

        [Test]
        public void should_return_audio_files_only()
        {
            GivenFiles(GetFiles(_path));

            Subject.GetAudioFiles(_path).Should().HaveCount(3);
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
            var files = GetFiles(_path).ToList();
            var specialFiles = GetFiles(_path, subFolder).ToList();
            var allFiles = files.Concat(specialFiles);

            var filteredFiles = Subject.FilterFiles(_path, allFiles);
            filteredFiles.Should().NotContain(specialFiles);
            filteredFiles.Count.Should().BeGreaterThan(0);
        }
    }
}
