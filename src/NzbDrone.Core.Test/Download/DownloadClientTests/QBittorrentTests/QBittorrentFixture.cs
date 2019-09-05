using System;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.QBittorrent;
using NzbDrone.Test.Common;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.QBittorrentTests
{
    [TestFixture]
    public class QBittorrentFixture : DownloadClientFixtureBase<QBittorrent>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new QBittorrentSettings
                                          {
                                              Host = "127.0.0.1",
                                              Port = 2222,
                                              Username = "admin",
                                              Password = "pass",
                                              MovieCategory = "movies-radarr"
                                          };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<Byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new Byte[0]));

            Mocker.GetMock<IQBittorrentProxy>()
                  .Setup(s => s.GetConfig(It.IsAny<QBittorrentSettings>()))
                  .Returns(new QBittorrentPreferences() { DhtEnabled = true });

            Mocker.GetMock<IQBittorrentProxySelector>()
                  .Setup(s => s.GetProxy(It.IsAny<QBittorrentSettings>(), It.IsAny<bool>()))
                  .Returns(Mocker.GetMock<IQBittorrentProxy>().Object);
        }

        protected void GivenRedirectToMagnet()
        {
            var httpHeader = new HttpHeader();
            httpHeader["Location"] = "magnet:?xt=urn:btih:ZPBPA2P6ROZPKRHK44D5OW6NHXU5Z6KR&tr=udp";

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, httpHeader, new Byte[0], System.Net.HttpStatusCode.SeeOther));
        }

        protected void GivenRedirectToTorrent()
        {
            var httpHeader = new HttpHeader();
            httpHeader["Location"] = "http://test.radarr.video/not-a-real-torrent.torrent";

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.Is<HttpRequest>(h => h.Url.FullUri == _downloadUrl)))
                  .Returns<HttpRequest>(r => new HttpResponse(r, httpHeader, new Byte[0], System.Net.HttpStatusCode.Found));
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IQBittorrentProxy>()
                .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<QBittorrentSettings>()))
                .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IQBittorrentProxy>()
                .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<QBittorrentSettings>()))
                .Callback(() =>
                {
                    var torrent = new QBittorrentTorrent
                    {
                        Hash = "HASH",
                        Name = _title,
                        Size = 1000,
                        Progress = 1.0,
                        Eta = 8640000,
                        State = "queuedUP",
                        Label = "",
                        SavePath = ""
                    };
                    GivenTorrents(new List<QBittorrentTorrent> { torrent });
                });
        }

        protected void GivenHighPriority()
        {
            Subject.Definition.Settings.As<QBittorrentSettings>().OlderMoviePriority = (int) QBittorrentPriority.First;
            Subject.Definition.Settings.As<QBittorrentSettings>().RecentMoviePriority = (int) QBittorrentPriority.First;
        }

        protected void GivenMaxRatio(float maxRatio, bool removeOnMaxRatio = true)
        {
            Mocker.GetMock<IQBittorrentProxy>()
                .Setup(s => s.GetConfig(It.IsAny<QBittorrentSettings>()))
                .Returns(new QBittorrentPreferences
                         {
                             RemoveOnMaxRatio = removeOnMaxRatio,
                             MaxRatio = maxRatio
                         });
        }

        protected virtual void GivenTorrents(List<QBittorrentTorrent> torrents)
        {
            if (torrents == null)
                torrents = new List<QBittorrentTorrent>();

            Mocker.GetMock<IQBittorrentProxy>()
                .Setup(s => s.GetTorrents(It.IsAny<QBittorrentSettings>()))
                .Returns(torrents);
        }

        [Test]
        public void error_item_should_have_required_properties()
        {
            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 0.7,
                Eta = 8640000,
                State = "error",
                Label = "",
                SavePath = ""
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            VerifyFailed(item);
        }

        [Test]
        public void paused_item_should_have_required_properties()
        {
            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 0.7,
                Eta = 8640000,
                State = "pausedDL",
                Label = "",
                SavePath = ""
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            VerifyPaused(item);
            item.RemainingTime.Should().NotHaveValue();
        }

        [TestCase("pausedUP")]
        [TestCase("queuedUP")]
        [TestCase("uploading")]
        [TestCase("stalledUP")]
        [TestCase("checkingUP")]
        [TestCase("forcedUP")]
        public void completed_item_should_have_required_properties(string state)
        {
            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 1.0,
                Eta = 8640000,
                State = state,
                Label = "",
                SavePath = ""
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            VerifyCompleted(item);
            item.RemainingTime.Should().Be(TimeSpan.Zero);
        }

        [TestCase("queuedDL")]
        [TestCase("checkingDL")]
        [TestCase("metaDL")]
        public void queued_item_should_have_required_properties(string state)
        {
            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 0.7,
                Eta = 8640000,
                State = state,
                Label = "",
                SavePath = ""
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            VerifyQueued(item);
            item.RemainingTime.Should().NotHaveValue();
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 0.7,
                Eta = 60,
                State = "downloading",
                Label = "",
                SavePath = ""
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            VerifyDownloading(item);
            item.RemainingTime.Should().NotBe(TimeSpan.Zero);
        }

        [Test]
        public void stalledDL_item_should_have_required_properties()
        {
            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 0.7,
                Eta = 8640000,
                State = "stalledDL",
                Label = "",
                SavePath = ""
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            VerifyWarning(item);
            item.RemainingTime.Should().NotHaveValue();
        }

        [Test]
        public void Download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();
        }

        [TestCase("magnet:?xt=urn:btih:ZPBPA2P6ROZPKRHK44D5OW6NHXU5Z6KR&tr=udp", "CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951")]
        public void Download_should_get_hash_from_magnet_url(string magnetUrl, string expectedHash)
        {
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.DownloadUrl = magnetUrl;

            var id = Subject.Download(remoteMovie);

            id.Should().Be(expectedHash);
        }

        [Test]
        public void Download_should_refuse_magnet_if_no_trackers_provided_and_dht_is_disabled()
        {
            Mocker.GetMock<IQBittorrentProxy>()
                  .Setup(s => s.GetConfig(It.IsAny<QBittorrentSettings>()))
                  .Returns(new QBittorrentPreferences() { DhtEnabled = false });

            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.DownloadUrl = "magnet:?xt=urn:btih:ZPBPA2P6ROZPKRHK44D5OW6NHXU5Z6KR";

            Assert.Throws<ReleaseDownloadException>(() => Subject.Download(remoteMovie));
        }

        [Test]
        public void Download_should_accept_magnet_if_trackers_provided_and_dht_is_disabled()
        {
            Mocker.GetMock<IQBittorrentProxy>()
                  .Setup(s => s.GetConfig(It.IsAny<QBittorrentSettings>()))
                  .Returns(new QBittorrentPreferences() { DhtEnabled = false });

            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.DownloadUrl = "magnet:?xt=urn:btih:ZPBPA2P6ROZPKRHK44D5OW6NHXU5Z6KR&tr=udp://abc";

            Assert.DoesNotThrow(() => Subject.Download(remoteMovie));

            Mocker.GetMock<IQBittorrentProxy>()
                  .Verify(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<QBittorrentSettings>()), Times.Once());
        }

        [Test]
        public void Download_should_set_top_priority()
        {
            GivenHighPriority();
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            Mocker.GetMock<IQBittorrentProxy>()
                  .Verify(v => v.MoveTorrentToTopInQueue(It.IsAny<string>(), It.IsAny<QBittorrentSettings>()), Times.Once());
        }

        [Test]
        public void Download_should_not_fail_if_top_priority_not_available()
        {
            GivenHighPriority();
            GivenSuccessfulDownload();

            Mocker.GetMock<IQBittorrentProxy>()
                  .Setup(v => v.MoveTorrentToTopInQueue(It.IsAny<string>(), It.IsAny<QBittorrentSettings>()))
                  .Throws(new HttpException(new HttpResponse(new HttpRequest("http://me.local/"), new HttpHeader(), new byte[0], System.Net.HttpStatusCode.Forbidden)));

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_status_with_outputdirs()
        {
            var config = new QBittorrentPreferences
            {
                SavePath = @"C:\Downloads\Finished\QBittorrent".AsOsAgnostic()
            };

            Mocker.GetMock<IQBittorrentProxy>()
                .Setup(v => v.GetConfig(It.IsAny<QBittorrentSettings>()))
                .Returns(config);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"C:\Downloads\Finished\QBittorrent".AsOsAgnostic());
        }

        [Test]
        public void Download_should_handle_http_redirect_to_magnet()
        {
            GivenRedirectToMagnet();
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Download_should_handle_http_redirect_to_torrent()
        {
            GivenRedirectToTorrent();
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void should_not_be_removable_and_should_not_allow_move_files_if_max_ratio_not_reached()
        {
            GivenMaxRatio(1.0f);

            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 1.0,
                Eta = 8640000,
                State = "uploading",
                Label = "",
                SavePath = "",
                Ratio = 0.5f
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            item.CanBeRemoved.Should().BeFalse();
            item.CanMoveFiles.Should().BeFalse();
        }

        [Test]
        public void should_not_be_removable_and_should_not_allow_move_files_if_max_ratio_reached_and_not_paused()
        {
            GivenMaxRatio(1.0f);

            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 1.0,
                Eta = 8640000,
                State = "uploading",
                Label = "",
                SavePath = "",
                Ratio = 1.0f
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            item.CanBeRemoved.Should().BeFalse();
            item.CanMoveFiles.Should().BeFalse();
        }

        [Test]
        public void should_not_be_removable_and_should_not_allow_move_files_if_max_ratio_is_not_set()
        {
            GivenMaxRatio(1.0f, false);

            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 1.0,
                Eta = 8640000,
                State = "uploading",
                Label = "",
                SavePath = "",
                Ratio = 1.0f
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            item.CanBeRemoved.Should().BeFalse();
            item.CanMoveFiles.Should().BeFalse();
        }

        [Test]
        public void should_be_removable_and_should_allow_move_files_if_max_ratio_reached_and_paused()
        {
            GivenMaxRatio(1.0f);

            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 1.0,
                Eta = 8640000,
                State = "pausedUP",
                Label = "",
                SavePath = "",
                Ratio = 1.0f
            };
            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            item.CanBeRemoved.Should().BeTrue();
            item.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void should_get_category_from_the_category_if_set()
        {
            const string category = "movies-radarr";
            GivenMaxRatio(1.0f);

            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 1.0,
                Eta = 8640000,
                State = "pausedUP",
                Category = category,
                SavePath = "",
                Ratio = 1.0f
            };

            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            item.Category.Should().Be(category);
        }

        [Test]
        public void should_get_category_from_the_label_if_the_category_is_not_available()
        {
            const string category = "movies-radarr";
            GivenMaxRatio(1.0f);

            var torrent = new QBittorrentTorrent
            {
                Hash = "HASH",
                Name = _title,
                Size = 1000,
                Progress = 1.0,
                Eta = 8640000,
                State = "pausedUP",
                Label = category,
                SavePath = "",
                Ratio = 1.0f
            };

            GivenTorrents(new List<QBittorrentTorrent> { torrent });

            var item = Subject.GetItems().Single();
            item.Category.Should().Be(category);
        }

        [Test]
        public void Test_should_force_api_version_check()
        {
            // Set TestConnection up to fail quick
            Mocker.GetMock<IQBittorrentProxy>()
                  .Setup(v => v.GetApiVersion(It.IsAny<QBittorrentSettings>()))
                  .Returns(new Version(1, 0));

            Subject.Test();

            Mocker.GetMock<IQBittorrentProxySelector>()
                  .Verify(v => v.GetProxy(It.IsAny<QBittorrentSettings>(), true), Times.Once());
        }
    }
}
