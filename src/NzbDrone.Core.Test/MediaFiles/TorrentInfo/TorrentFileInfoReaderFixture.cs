using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using MonoTorrent;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.TorrentInfo
{
    [TestFixture]
    public class TorrentFileInfoReaderFixture : TestBase<TorrentFileInfoReader>
    {
        private byte[] CreateSingleFileTorrent(string filename)
        {
            var creator = new TorrentCreator();
            var files = new List<TorrentFile>
            {
                new TorrentFile(filename, 1024 * 1024 * 100) // 100 MB
            };

            var torrent = creator.Create(new TorrentCreatorAsyncResult(new CreateTorrentOptions(files, 256 * 1024)));
            using (var ms = new MemoryStream())
            {
                torrent.SaveTo(ms);
                return ms.ToArray();
            }
        }

        private byte[] CreateMultiFileTorrent(params string[] filenames)
        {
            var creator = new TorrentCreator();
            var files = filenames.Select(f => new TorrentFile(f, 1024 * 1024 * 50)).ToList();

            var torrent = creator.Create(new TorrentCreatorAsyncResult(new CreateTorrentOptions(files, 256 * 1024)));
            using (var ms = new MemoryStream())
            {
                torrent.SaveTo(ms);
                return ms.ToArray();
            }
        }

        [Test]
        public void should_detect_single_video_file()
        {
            // Arrange
            var torrentBytes = CreateSingleFileTorrent("Movie.2024.1080p.mkv");

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.Should().NotBeNull();
            info.IsSingleFile.Should().BeTrue();
            info.FileCount.Should().Be(1);
            info.ContainsVideoFile.Should().BeTrue();
            info.ContainsArchives.Should().BeFalse();
            info.VideoFileName.Should().Be("Movie.2024.1080p.mkv");
        }

        [Test]
        public void should_detect_multiple_files()
        {
            // Arrange
            var torrentBytes = CreateMultiFileTorrent(
                "Movie.2024.1080p.mkv",
                "sample.mkv",
                "info.txt"
            );

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.Should().NotBeNull();
            info.IsSingleFile.Should().BeFalse();
            info.FileCount.Should().Be(3);
            info.ContainsVideoFile.Should().BeTrue();
        }

        [Test]
        public void should_detect_archived_content()
        {
            // Arrange
            var torrentBytes = CreateSingleFileTorrent("Movie.2024.1080p.rar");

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.Should().NotBeNull();
            info.ContainsArchives.Should().BeTrue();
            info.ContainsVideoFile.Should().BeFalse();
        }

        [TestCase(".mkv")]
        [TestCase(".mp4")]
        [TestCase(".avi")]
        [TestCase(".mov")]
        [TestCase(".wmv")]
        [TestCase(".m4v")]
        [TestCase(".mpg")]
        [TestCase(".mpeg")]
        [TestCase(".ts")]
        [TestCase(".m2ts")]
        public void should_recognize_video_extensions(string extension)
        {
            // Arrange
            var filename = $"Movie.2024.1080p{extension}";
            var torrentBytes = CreateSingleFileTorrent(filename);

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.ContainsVideoFile.Should().BeTrue();
            info.VideoFileName.Should().Be(filename);
        }

        [TestCase(".rar")]
        [TestCase(".zip")]
        [TestCase(".7z")]
        [TestCase(".tar")]
        [TestCase(".gz")]
        [TestCase(".bz2")]
        [TestCase(".r00")]
        [TestCase(".r01")]
        public void should_recognize_archive_extensions(string extension)
        {
            // Arrange
            var filename = $"Movie.2024.1080p{extension}";
            var torrentBytes = CreateSingleFileTorrent(filename);

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.ContainsArchives.Should().BeTrue();
        }

        [Test]
        public void should_identify_first_video_file_in_multifile_torrent()
        {
            // Arrange
            var torrentBytes = CreateMultiFileTorrent(
                "info.nfo",
                "Movie.2024.1080p.mkv",
                "sample.mkv"
            );

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.VideoFileName.Should().Be("Movie.2024.1080p.mkv");
        }

        [Test]
        public void should_return_total_size()
        {
            // Arrange
            var torrentBytes = CreateSingleFileTorrent("Movie.2024.1080p.mkv");

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.TotalSize.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_return_all_files_list()
        {
            // Arrange
            var filenames = new[] { "Movie.2024.1080p.mkv", "sample.mkv", "info.txt" };
            var torrentBytes = CreateMultiFileTorrent(filenames);

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.Files.Should().NotBeEmpty();
            info.Files.Count.Should().Be(3);
            info.Files.Should().Contain(filenames);
        }

        [Test]
        public void should_handle_mixed_archive_and_video_content()
        {
            // Arrange
            var torrentBytes = CreateMultiFileTorrent(
                "Movie.2024.1080p.mkv",
                "extras.rar"
            );

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.ContainsVideoFile.Should().BeTrue();
            info.ContainsArchives.Should().BeTrue();
        }

        [Test]
        public void should_handle_no_video_files()
        {
            // Arrange
            var torrentBytes = CreateMultiFileTorrent(
                "info.nfo",
                "readme.txt"
            );

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.ContainsVideoFile.Should().BeFalse();
            info.VideoFileName.Should().BeNull();
        }

        [Test]
        public void should_be_case_insensitive_for_extensions()
        {
            // Arrange
            var torrentBytes = CreateSingleFileTorrent("Movie.2024.1080p.MKV");

            // Act
            var info = Subject.GetTorrentInfo(torrentBytes);

            // Assert
            info.ContainsVideoFile.Should().BeTrue();
        }
    }
}
