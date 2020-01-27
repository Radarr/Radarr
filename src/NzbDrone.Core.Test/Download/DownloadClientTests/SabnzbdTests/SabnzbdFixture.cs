using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Download.Clients.Sabnzbd.Responses;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.SabnzbdTests
{
    [TestFixture]
    public class SabnzbdFixture : DownloadClientFixtureBase<Sabnzbd>
    {
        private SabnzbdQueue _queued;
        private SabnzbdHistory _failed;
        private SabnzbdHistory _completed;
        private SabnzbdConfig _config;
        private SabnzbdFullStatus _fullStatus;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new SabnzbdSettings
            {
                Host = "127.0.0.1",
                Port = 2222,
                ApiKey = "5c770e3197e4fe763423ee7c392c25d1",
                Username = "admin",
                Password = "pass",
                MovieCategory = "movie",
                RecentMoviePriority = (int)SabnzbdPriority.High
            };
            _queued = new SabnzbdQueue
            {
                DefaultRootFolder = @"Y:\nzbget\root".AsOsAgnostic(),
                Paused = false,
                Items = new List<SabnzbdQueueItem>()
                    {
                        new SabnzbdQueueItem
                        {
                            Status = SabnzbdDownloadStatus.Downloading,
                            Size = 1000,
                            Sizeleft = 10,
                            Timeleft = TimeSpan.FromSeconds(10),
                            Category = "movie",
                            Id = "sabnzbd_nzb12345",
                            Title = "Droned.1998.1080p.WEB-DL-DRONE"
                        }
                    }
            };

            _failed = new SabnzbdHistory
            {
                Items = new List<SabnzbdHistoryItem>()
                    {
                        new SabnzbdHistoryItem
                        {
                            Status = SabnzbdDownloadStatus.Failed,
                            Size = 1000,
                            Category = "movie",
                            Id = "sabnzbd_nzb12345",
                            Title = "Droned.1998.1080p.WEB-DL-DRONE"
                        }
                    }
            };

            _completed = new SabnzbdHistory
            {
                Items = new List<SabnzbdHistoryItem>()
                    {
                        new SabnzbdHistoryItem
                        {
                            Status = SabnzbdDownloadStatus.Completed,
                            Size = 1000,
                            Category = "movie",
                            Id = "sabnzbd_nzb12345",
                            Title = "Droned.1998.1080p.WEB-DL-DRONE",
                            Storage = "/remote/mount/vv/Droned.1998.1080p.WEB-DL-DRONE"
                        }
                    }
            };

            _config = new SabnzbdConfig
            {
                Misc = new SabnzbdConfigMisc
                {
                    complete_dir = @"/remote/mount"
                },
                Categories = new List<SabnzbdCategory>
                        {
                            new SabnzbdCategory  { Name = "movie", Dir = "vv" }
                        }
            };

            Mocker.GetMock<ISabnzbdProxy>()
                  .Setup(v => v.GetVersion(It.IsAny<SabnzbdSettings>()))
                  .Returns("1.2.3");

            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetConfig(It.IsAny<SabnzbdSettings>()))
                .Returns(_config);

            _fullStatus = new SabnzbdFullStatus
            {
                CompleteDir = @"Y:\sabnzbd\root\complete".AsOsAgnostic()
            };

            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetFullStatus(It.IsAny<SabnzbdSettings>()))
                .Returns(_fullStatus);
        }

        protected void GivenVersion(string version)
        {
            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetVersion(It.IsAny<SabnzbdSettings>()))
                .Returns(version);
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SabnzbdSettings>()))
                .Returns((SabnzbdAddResponse)null);
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SabnzbdSettings>()))
                .Returns(new SabnzbdAddResponse()
                {
                    Status = true,
                    Ids = new List<string> { "sabznbd_nzo12345" }
                });
        }

        protected virtual void GivenQueue(SabnzbdQueue queue)
        {
            if (queue == null)
            {
                queue = new SabnzbdQueue()
                {
                    DefaultRootFolder = _queued.DefaultRootFolder,
                    Items = new List<SabnzbdQueueItem>()
                };
            }

            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetQueue(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SabnzbdSettings>()))
                .Returns(queue);
        }

        protected virtual void GivenHistory(SabnzbdHistory history)
        {
            if (history == null)
            {
                history = new SabnzbdHistory() { Items = new List<SabnzbdHistoryItem>() };
            }

            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetHistory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SabnzbdSettings>()))
                .Returns(history);
        }

        [Test]
        public void GetItems_should_return_no_items_when_queue_is_empty()
        {
            GivenQueue(null);
            GivenHistory(null);

            Subject.GetItems().Should().BeEmpty();
        }

        [TestCase(SabnzbdDownloadStatus.Grabbing)]
        [TestCase(SabnzbdDownloadStatus.Queued)]
        public void queued_item_should_have_required_properties(SabnzbdDownloadStatus status)
        {
            _queued.Items.First().Status = status;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyQueued(result);

            result.RemainingTime.Should().NotBe(TimeSpan.Zero);
            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [TestCase(SabnzbdDownloadStatus.Paused)]
        public void paused_item_should_have_required_properties(SabnzbdDownloadStatus status)
        {
            _queued.Items.First().Status = status;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyPaused(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [TestCase(SabnzbdDownloadStatus.Checking)]
        [TestCase(SabnzbdDownloadStatus.Downloading)]
        [TestCase(SabnzbdDownloadStatus.QuickCheck)]
        [TestCase(SabnzbdDownloadStatus.ToPP)]
        [TestCase(SabnzbdDownloadStatus.Verifying)]
        [TestCase(SabnzbdDownloadStatus.Repairing)]
        [TestCase(SabnzbdDownloadStatus.Fetching)]
        [TestCase(SabnzbdDownloadStatus.Extracting)]
        [TestCase(SabnzbdDownloadStatus.Moving)]
        [TestCase(SabnzbdDownloadStatus.Running)]
        public void downloading_item_should_have_required_properties(SabnzbdDownloadStatus status)
        {
            _queued.Items.First().Status = status;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyDownloading(result);

            result.RemainingTime.Should().NotBe(TimeSpan.Zero);
            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            VerifyCompleted(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void failed_item_should_have_required_properties()
        {
            _completed.Items.First().Status = SabnzbdDownloadStatus.Failed;

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            VerifyFailed(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void deleted_queue_item_should_be_ignored()
        {
            _queued.Items.First().Status = SabnzbdDownloadStatus.Deleted;

            GivenQueue(_queued);
            GivenHistory(null);

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void deleted_history_item_should_be_ignored()
        {
            _completed.Items.First().Status = SabnzbdDownloadStatus.Deleted;

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.GetItems().Should().BeEmpty();
        }

        [TestCase("[ TOWN ]-[ http://www.town.ag ]-[ ANIME ]-[Usenet Provider >> http://www.ssl- <<] - [Commie] Aldnoah Zero 18 [234C8FC7]", "[ TOWN ]-[ http++www.town.ag ]-[ ANIME ]-[Usenet Provider  http++www.ssl- ] - [Commie] Aldnoah Zero 18 [234C8FC7].nzb")]
        public void Download_should_use_clean_title(string title, string filename)
        {
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();
            remoteMovie.Release.Title = title;

            var id = Subject.Download(remoteMovie);

            Mocker.GetMock<ISabnzbdProxy>()
                .Verify(v => v.DownloadNzb(It.IsAny<byte[]>(), filename, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SabnzbdSettings>()), Times.Once());
        }

        [Test]
        public void Download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetItems_should_ignore_downloads_from_other_categories()
        {
            _completed.Items.First().Category = "myowncat";

            GivenQueue(null);
            GivenHistory(_completed);

            var items = Subject.GetItems();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_report_diskspace_unpack_error_as_warning()
        {
            _completed.Items.First().FailMessage = "Unpacking failed, write error or disk is full?";
            _completed.Items.First().Status = SabnzbdDownloadStatus.Failed;

            GivenQueue(null);
            GivenHistory(_completed);

            var items = Subject.GetItems();

            items.First().Status.Should().Be(DownloadItemStatus.Warning);
        }

        [Test]
        [Ignore("Series")]
        public void Download_should_use_sabRecentTvPriority_when_recentEpisode_is_true()
        {
            Mocker.GetMock<ISabnzbdProxy>()
                    .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), (int)SabnzbdPriority.High, It.IsAny<SabnzbdSettings>()))
                    .Returns(new SabnzbdAddResponse());

            var remoteMovie = CreateRemoteMovie();
            /*remoteMovie.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                      .All()
                                                      .With(e => e.AirDate = DateTime.Today.ToString(Episode.AIR_DATE_FORMAT))
                                                      .Build()
                                                      .ToList();*/

            Subject.Download(remoteMovie);

            Mocker.GetMock<ISabnzbdProxy>()
                  .Verify(v => v.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), (int)SabnzbdPriority.High, It.IsAny<SabnzbdSettings>()), Times.Once());
        }

        [TestCase(@"Droned.1998.1080p.WEB-DL-DRONE", @"Droned.1998_1080p_WEB-DL-DRONE.mkv")]
        [TestCase(@"Droned.1998.1080p.WEB-DL-DRONE", @"SubDir\Droned.1998_1080p_WEB-DL-DRONE.mkv")]
        [TestCase(@"Droned.1998.1080p.WEB-DL-DRONE.mkv", @"SubDir\Droned.1998_1080p_WEB-DL-DRONE.mkv")]
        [TestCase(@"Droned.1998.1080p.WEB-DL-DRONE.mkv", @"SubDir\SubDir\Droned.1998_1080p_WEB-DL-DRONE.mkv")]
        public void should_return_path_to_jobfolder(string title, string storage)
        {
            _completed.Items.First().Title = title;
            _completed.Items.First().Storage = (@"C:\sorted\" + title + @"\" + storage).AsOsAgnostic();

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(new OsPath((@"C:\sorted\" + title).AsOsAgnostic()).AsDirectory());
        }

        [Test]
        public void should_remap_storage_if_mounted()
        {
            Mocker.GetMock<IRemotePathMappingService>()
                .Setup(v => v.RemapRemoteToLocal("127.0.0.1", It.IsAny<OsPath>()))
                .Returns(new OsPath(@"O:\mymount\Droned.1998.1080p.WEB-DL-DRONE".AsOsAgnostic()));

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(@"O:\mymount\Droned.1998.1080p.WEB-DL-DRONE".AsOsAgnostic());
        }

        [Test]
        public void should_not_blow_up_if_storage_is_drive_root()
        {
            _completed.Items.First().Storage = @"C:\".AsOsAgnostic();

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(@"C:\".AsOsAgnostic());
        }

        [Test]
        public void should_not_blow_up_if_storage_doesnt_have_jobfolder()
        {
            _completed.Items.First().Storage = @"C:\sorted\somewhere\asdfasdf\asdfasdf.mkv".AsOsAgnostic();

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(@"C:\sorted\somewhere\asdfasdf\asdfasdf.mkv".AsOsAgnostic());
        }

        [TestCase(@"Y:\sabnzbd\root", @"completed\downloads", @"vv", @"Y:\sabnzbd\root\completed\downloads", @"Y:\sabnzbd\root\completed\downloads\vv")]
        [TestCase(@"Y:\sabnzbd\root", @"completed", @"vv", @"Y:\sabnzbd\root\completed", @"Y:\sabnzbd\root\completed\vv")]
        [TestCase(@"/sabnzbd/root", @"completed/downloads", @"vv", @"/sabnzbd/root/completed/downloads", @"/sabnzbd/root/completed/downloads/vv")]
        [TestCase(@"/sabnzbd/root", @"completed", @"vv", @"/sabnzbd/root/completed", @"/sabnzbd/root/completed/vv")]
        public void should_return_status_with_outputdir_for_version_lt_2(string rootFolder, string completeDir, string categoryDir, string fullCompleteDir, string fullCategoryDir)
        {
            _fullStatus.CompleteDir = null;
            _queued.DefaultRootFolder = rootFolder;
            _config.Misc.complete_dir = completeDir;
            _config.Categories.First().Dir = categoryDir;

            GivenVersion("1.2.1");
            GivenQueue(null);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(fullCategoryDir);
        }

        [TestCase(@"Y:\sabnzbd\root", @"completed\downloads", @"vv", @"Y:\sabnzbd\root\completed\downloads", @"Y:\sabnzbd\root\completed\downloads\vv")]
        [TestCase(@"Y:\sabnzbd\root", @"completed", @"vv", @"Y:\sabnzbd\root\completed", @"Y:\sabnzbd\root\completed\vv")]
        [TestCase(@"/sabnzbd/root", @"completed/downloads", @"vv", @"/sabnzbd/root/completed/downloads", @"/sabnzbd/root/completed/downloads/vv")]
        [TestCase(@"/sabnzbd/root", @"completed", @"vv", @"/sabnzbd/root/completed", @"/sabnzbd/root/completed/vv")]
        public void should_return_status_with_outputdir_for_version_gte_2(string rootFolder, string completeDir, string categoryDir, string fullCompleteDir, string fullCategoryDir)
        {
            _fullStatus.CompleteDir = fullCompleteDir;
            _queued.DefaultRootFolder = null;
            _config.Misc.complete_dir = completeDir;
            _config.Categories.First().Dir = categoryDir;

            GivenVersion("2.0.0beta1");
            GivenQueue(null);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(fullCategoryDir);
        }

        [Test]
        public void should_return_status_with_mounted_outputdir()
        {
            Mocker.GetMock<IRemotePathMappingService>()
                .Setup(v => v.RemapRemoteToLocal("127.0.0.1", It.IsAny<OsPath>()))
                .Returns(new OsPath(@"O:\mymount".AsOsAgnostic()));

            GivenQueue(null);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"O:\mymount".AsOsAgnostic());
        }

        [TestCase("0.6.9", false)]
        [TestCase("0.7.0", true)]
        [TestCase("0.8.0", true)]
        [TestCase("1.0.0", true)]
        [TestCase("1.0.0RC1", true)]
        [TestCase("1.1.x", true)]
        public void should_test_version(string version, bool expected)
        {
            Mocker.GetMock<ISabnzbdProxy>()
                  .Setup(v => v.GetVersion(It.IsAny<SabnzbdSettings>()))
                  .Returns(version);

            var error = Subject.Test();

            error.IsValid.Should().Be(expected);
        }

        [Test]
        public void should_test_develop_version_successfully()
        {
            Mocker.GetMock<ISabnzbdProxy>()
                  .Setup(v => v.GetVersion(It.IsAny<SabnzbdSettings>()))
                  .Returns("develop");

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
        }

        [Test]
        public void should_test_success_if_tv_sorting_disabled()
        {
            _config.Misc.enable_tv_sorting = false;
            _config.Misc.tv_categories = null;

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void should_test_failed_if_tv_sorting_null()
        {
            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = null;

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void should_test_failed_if_tv_sorting_empty()
        {
            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = new string[0];

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void should_test_success_if_tv_sorting_contains_different_category()
        {
            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = new[] { "tv-custom" };

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void should_test_failed_if_tv_sorting_contains_category()
        {
            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = new[] { "movie" };

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void should_test_failed_if_tv_sorting_default_category()
        {
            Subject.Definition.Settings.As<SabnzbdSettings>().MovieCategory = null;

            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = new[] { "Default" };

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeFalse();
        }
    }
}
