using System;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Test.Common;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.NzbVortex;
using NzbDrone.Core.Download.Clients.NzbVortex.Responses;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.NzbVortexTests
{
    [TestFixture]
    public class NzbVortexFixture : DownloadClientFixtureBase<NzbVortex>
    {
        private NzbVortexQueueItem _queued;
        private NzbVortexQueueItem _failed;
        private NzbVortexQueueItem _completed;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new NzbVortexSettings
                                          {
                                              Host = "127.0.0.1",
                                              Port = 2222,
                                              ApiKey = "1234-ABCD",
                                              MusicCategory = "Music",
                                              RecentTvPriority = (int)NzbgetPriority.High
                                          };

            _queued = new NzbVortexQueueItem
                {
                    Id = RandomNumber,
                    DownloadedSize = 1000,
                    TotalDownloadSize = 10,
                    GroupName = "Music",
                    UiTitle = "Fall Out Boy-Make America Psycho Again-CD-FLAC-2015-FORSAKEN"
            };

            _failed = new NzbVortexQueueItem
                {
                    DownloadedSize = 1000,
                    TotalDownloadSize = 1000,
                    GroupName = "Music",
                    UiTitle = "Fall Out Boy-Make America Psycho Again-CD-FLAC-2015-FORSAKEN",
                    DestinationPath = "somedirectory",
                    State =  NzbVortexStateType.UncompressFailed,
                };

            _completed = new NzbVortexQueueItem
                {
                    DownloadedSize = 1000,
                    TotalDownloadSize = 1000,
                    GroupName = "Music",
                    UiTitle = "Fall Out Boy-Make America Psycho Again-CD-FLAC-2015-FORSAKEN",
                    DestinationPath = "/remote/mount/music/Fall Out Boy-Make America Psycho Again-CD-FLAC-2015-FORSAKEN",
                    State = NzbVortexStateType.Done
                };
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<INzbVortexProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<NzbVortexSettings>()))
                .Returns((string)null);
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<INzbVortexProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<NzbVortexSettings>()))
                .Returns(Guid.NewGuid().ToString().Replace("-", ""));
        }

        protected virtual void GivenQueue(NzbVortexQueueItem queue)
        {
            var list = new List<NzbVortexQueueItem>();

            list.AddIfNotNull(queue);

            Mocker.GetMock<INzbVortexProxy>()
                .Setup(s => s.GetQueue(It.IsAny<int>(), It.IsAny<NzbVortexSettings>()))
                .Returns(list);
        }

        [Test]
        public void GetItems_should_return_no_items_when_queue_is_empty()
        {
            GivenQueue(null);

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void queued_item_should_have_required_properties()
        {
            GivenQueue(_queued);
            
            var result = Subject.GetItems().Single();

            VerifyQueued(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void paused_item_should_have_required_properties()
        {
            _queued.IsPaused = true;
            GivenQueue(_queued);

            var result = Subject.GetItems().Single();

            VerifyPaused(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            _queued.State = NzbVortexStateType.Downloading;
            GivenQueue(_queued);

            var result = Subject.GetItems().Single();

            VerifyDownloading(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            GivenQueue(_completed);

            var result = Subject.GetItems().Single();

            VerifyCompleted(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void failed_item_should_have_required_properties()
        {
            GivenQueue(_failed);

            var result = Subject.GetItems().Single();

            VerifyFailed(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void should_report_UncompressFailed_as_failed()
        {
            _queued.State = NzbVortexStateType.UncompressFailed;
            GivenQueue(_failed);

            var items = Subject.GetItems();

            items.First().Status.Should().Be(DownloadItemStatus.Failed);
        }

        [Test]
        public void should_report_CheckFailedDataCorrupt_as_failed()
        {
            _queued.State = NzbVortexStateType.CheckFailedDataCorrupt;
            GivenQueue(_failed);

            var result = Subject.GetItems().Single();

            result.Status.Should().Be(DownloadItemStatus.Failed);
        }

        [Test]
        public void should_report_BadlyEncoded_as_failed()
        {
            _queued.State = NzbVortexStateType.BadlyEncoded;
            GivenQueue(_failed);

            var items = Subject.GetItems();

            items.First().Status.Should().Be(DownloadItemStatus.Failed);
        }

        [Test]
        public void Download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteAlbum = CreateRemoteAlbum();

            var id = Subject.Download(remoteAlbum);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Download_should_throw_if_failed()
        {
            GivenFailedDownload();

            var remoteAlbum = CreateRemoteAlbum();

            Assert.Throws<DownloadClientException>(() => Subject.Download(remoteAlbum));
        }

        [Test]
        public void GetItems_should_ignore_downloads_from_other_categories()
        {
            _completed.GroupName = "mycat";

            GivenQueue(null);

            var items = Subject.GetItems();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_remap_storage_if_mounted()
        {
            Mocker.GetMock<IRemotePathMappingService>()
                  .Setup(v => v.RemapRemoteToLocal("127.0.0.1", It.IsAny<OsPath>()))
                  .Returns(new OsPath(@"O:\mymount\Fall Out Boy-Make America Psycho Again-CD-FLAC-2015-FORSAKEN".AsOsAgnostic()));

            GivenQueue(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(@"O:\mymount\Fall Out Boy-Make America Psycho Again-CD-FLAC-2015-FORSAKEN".AsOsAgnostic());
        }

        [Test]
        public void should_get_files_if_completed_download_is_not_in_a_job_folder()
        {
            Mocker.GetMock<IRemotePathMappingService>()
                  .Setup(v => v.RemapRemoteToLocal("127.0.0.1", It.IsAny<OsPath>()))
                  .Returns(new OsPath(@"O:\mymount\".AsOsAgnostic()));

            Mocker.GetMock<INzbVortexProxy>()
                  .Setup(s => s.GetFiles(It.IsAny<int>(), It.IsAny<NzbVortexSettings>()))
                  .Returns(new List<NzbVortexFile> { new NzbVortexFile { FileName = "Fall Out Boy - Make America Psyco Again - Track 1.flac" } });

            _completed.State = NzbVortexStateType.Done;
            GivenQueue(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(@"O:\mymount\Fall Out Boy - Make America Psyco Again - Track 1.flac".AsOsAgnostic());
        }

        [Test]
        public void should_be_warning_if_more_than_one_file_is_not_in_a_job_folder()
        {
            Mocker.GetMock<IRemotePathMappingService>()
                  .Setup(v => v.RemapRemoteToLocal("127.0.0.1", It.IsAny<OsPath>()))
                  .Returns(new OsPath(@"O:\mymount\".AsOsAgnostic()));

            Mocker.GetMock<INzbVortexProxy>()
                  .Setup(s => s.GetFiles(It.IsAny<int>(), It.IsAny<NzbVortexSettings>()))
                  .Returns(new List<NzbVortexFile>
                           {
                               new NzbVortexFile { FileName = "Fall Out Boy - Make America Psyco Again - Track 1.flac" },
                               new NzbVortexFile { FileName = "Fall Out Boy-Make America Psycho Again-CD-FLAC-2015-FORSAKEN.nfo" }
                           });

            _completed.State = NzbVortexStateType.Done;
            GivenQueue(_completed);

            var result = Subject.GetItems().Single();

            result.Status.Should().Be(DownloadItemStatus.Warning);
        }

        [TestCase("1.0", false)]
        [TestCase("2.2", false)]
        [TestCase("2.3", true)]
        [TestCase("2.4", true)]
        [TestCase("3.0", true)]
        public void should_test_api_version(string version, bool expected)
        {
            Mocker.GetMock<INzbVortexProxy>()
                .Setup(v => v.GetGroups(It.IsAny<NzbVortexSettings>()))
                .Returns(new List<NzbVortexGroup> { new NzbVortexGroup { GroupName = ((NzbVortexSettings)Subject.Definition.Settings).MusicCategory } });

            Mocker.GetMock<INzbVortexProxy>()
                .Setup(v => v.GetApiVersion(It.IsAny<NzbVortexSettings>()))
                .Returns(new NzbVortexApiVersionResponse { ApiLevel = version });

            var error = Subject.Test();

            error.IsValid.Should().Be(expected);
        }
    }
}
