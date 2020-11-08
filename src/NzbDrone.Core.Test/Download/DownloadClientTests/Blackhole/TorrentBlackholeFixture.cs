using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Blackhole;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.Blackhole
{
    [TestFixture]
    public class TorrentBlackholeFixture : DownloadClientFixtureBase<TorrentBlackhole>
    {
        protected string _completedDownloadFolder;
        protected string _blackholeFolder;
        protected string _filePath;
        protected string _magnetFilePath;

        [SetUp]
        public void Setup()
        {
            _completedDownloadFolder = @"c:\blackhole\completed".AsOsAgnostic();
            _blackholeFolder = @"c:\blackhole\torrent".AsOsAgnostic();
            _filePath = (@"c:\blackhole\torrent\" + _title + ".torrent").AsOsAgnostic();
            _magnetFilePath = Path.ChangeExtension(_filePath, ".magnet");

            Mocker.SetConstant<IScanWatchFolder>(Mocker.Resolve<ScanWatchFolder>());

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new TorrentBlackholeSettings
            {
                TorrentFolder = _blackholeFolder,
                WatchFolder = _completedDownloadFolder
            };

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.OpenWriteStream(It.IsAny<string>()))
                .Returns(() => new FileStream(GetTempFilePath(), FileMode.Create));

            Mocker.GetMock<ITorrentFileInfoReader>()
                .Setup(c => c.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                .Returns("myhash");

            Mocker.GetMock<IDiskScanService>().Setup(c => c.FilterPaths(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()))
                  .Returns<string, IEnumerable<string>, bool>((b, s, e) => s.ToList());
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                .Throws(new WebException());
        }

        protected void GivenCompletedItem()
        {
            var targetDir = Path.Combine(_completedDownloadFolder, _title);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetDirectories(_completedDownloadFolder))
                .Returns(new[] { targetDir });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFiles(targetDir, SearchOption.AllDirectories))
                .Returns(new[] { Path.Combine(targetDir, "somefile.mkv") });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFileSize(It.IsAny<string>()))
                .Returns(1000000);
        }

        protected void GivenMagnetFilePath(string extension = ".magnet")
        {
            _magnetFilePath = Path.ChangeExtension(_filePath, extension);
        }

        protected override RemoteMovie CreateRemoteMovie()
        {
            var remoteMovie = base.CreateRemoteMovie();
            var torrentInfo = new TorrentInfo();

            torrentInfo.Title = remoteMovie.Release.Title;
            torrentInfo.DownloadUrl = remoteMovie.Release.DownloadUrl;
            torrentInfo.DownloadProtocol = remoteMovie.Release.DownloadProtocol;
            torrentInfo.MagnetUrl = "magnet:?xt=urn:btih:755248817d32b00cc853e633ecdc48e4c21bff15&dn=Series.S05E10.PROPER.HDTV.x264-DEFiNE%5Brartv%5D&tr=http%3A%2F%2Ftracker.trackerfix.com%3A80%2Fannounce&tr=udp%3A%2F%2F9.rarbg.me%3A2710&tr=udp%3A%2F%2F9.rarbg.to%3A2710";

            remoteMovie.Release = torrentInfo;

            return remoteMovie;
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            Subject.ScanGracePeriod = TimeSpan.Zero;

            GivenCompletedItem();

            var result = Subject.GetItems().Single();

            VerifyCompleted(result);

            result.CanBeRemoved.Should().BeFalse();
            result.CanMoveFiles.Should().BeFalse();
        }

        [Test]
        public void partial_download_should_have_required_properties()
        {
            GivenCompletedItem();

            var result = Subject.GetItems().Single();

            VerifyPostprocessing(result);
        }

        [Test]
        public void should_return_category()
        {
            GivenCompletedItem();

            var result = Subject.GetItems().Single();

            // We must have a category or CDH won't pick it up.
            result.Category.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void Download_should_download_file_if_it_doesnt_exist()
        {
            var remoteMovie = CreateRemoteMovie();

            Subject.Download(remoteMovie);

            Mocker.GetMock<IHttpClient>().Verify(c => c.Get(It.Is<HttpRequest>(v => v.Url.FullUri == _downloadUrl)), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_filePath), Times.Once());
            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Download_should_save_magnet_if_enabled()
        {
            GivenMagnetFilePath();
            Subject.Definition.Settings.As<TorrentBlackholeSettings>().SaveMagnetFiles = true;

            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.DownloadUrl = null;

            Subject.Download(remoteMovie);

            Mocker.GetMock<IHttpClient>().Verify(c => c.Get(It.Is<HttpRequest>(v => v.Url.FullUri == _downloadUrl)), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_filePath), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_magnetFilePath), Times.Once());
            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Download_should_save_magnet_using_specified_extension()
        {
            var magnetFileExtension = ".url";
            GivenMagnetFilePath(magnetFileExtension);
            Subject.Definition.Settings.As<TorrentBlackholeSettings>().SaveMagnetFiles = true;
            Subject.Definition.Settings.As<TorrentBlackholeSettings>().MagnetFileExtension = magnetFileExtension;

            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.DownloadUrl = null;

            Subject.Download(remoteMovie);

            Mocker.GetMock<IHttpClient>().Verify(c => c.Get(It.Is<HttpRequest>(v => v.Url.FullUri == _downloadUrl)), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_filePath), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_magnetFilePath), Times.Once());
            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Download_should_not_save_magnet_if_disabled()
        {
            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.DownloadUrl = null;

            Assert.Throws<ReleaseDownloadException>(() => Subject.Download(remoteMovie));

            Mocker.GetMock<IHttpClient>().Verify(c => c.Get(It.Is<HttpRequest>(v => v.Url.FullUri == _downloadUrl)), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_filePath), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_magnetFilePath), Times.Never());
            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Download_should_prefer_torrent_over_magnet()
        {
            Subject.Definition.Settings.As<TorrentBlackholeSettings>().SaveMagnetFiles = true;

            var remoteMovie = CreateRemoteMovie();

            Subject.Download(remoteMovie);

            Mocker.GetMock<IHttpClient>().Verify(c => c.Get(It.Is<HttpRequest>(v => v.Url.FullUri == _downloadUrl)), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_filePath), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(_magnetFilePath), Times.Never());
            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Download_should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(_blackholeFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV]" + Path.GetExtension(_filePath));

            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.Title = illegalTitle;

            Subject.Download(remoteMovie);

            Mocker.GetMock<IHttpClient>().Verify(c => c.Get(It.Is<HttpRequest>(v => v.Url.FullUri == _downloadUrl)), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenWriteStream(expectedFilename), Times.Once());
            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Download_should_throw_if_magnet_and_torrent_url_does_not_exist()
        {
            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.DownloadUrl = null;

            Assert.Throws<ReleaseDownloadException>(() => Subject.Download(remoteMovie));
        }

        [Test]
        public void RemoveItem_should_delete_file()
        {
            GivenCompletedItem();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(true);

            Subject.RemoveItem("_Droned.1998.1080p.WEB-DL-DRONE_0", true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFile(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void RemoveItem_should_delete_directory()
        {
            GivenCompletedItem();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(true);

            Subject.RemoveItem("_Droned.1998.1080p.WEB-DL-DRONE_0", true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Once());
        }

        [Test]
        public void RemoveItem_should_ignore_if_unknown_item()
        {
            Subject.RemoveItem("_Droned.1998.1080p.WEB-DL-DRONE_0", true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFile(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void RemoveItem_should_throw_if_deleteData_is_false()
        {
            GivenCompletedItem();

            Assert.Throws<NotSupportedException>(() => Subject.RemoveItem("_Droned.1998.1080p.WEB-DL-DRONE_0", false));

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFile(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_return_status_with_outputdirs()
        {
            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(_completedDownloadFolder);
        }

        [Test]
        public void should_return_null_hash()
        {
            var remoteMovie = CreateRemoteMovie();

            Subject.Download(remoteMovie).Should().BeNull();
        }
    }
}
