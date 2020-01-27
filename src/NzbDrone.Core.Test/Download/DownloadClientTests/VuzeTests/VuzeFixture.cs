﻿using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Transmission;
using NzbDrone.Core.Download.Clients.Vuze;
using NzbDrone.Core.Test.Download.DownloadClientTests.TransmissionTests;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.VuzeTests
{
    [TestFixture]
    public class VuzeFixture : TransmissionFixtureBase<Vuze>
    {
        [SetUp]
        public void Setup_Vuze()
        {
            // Vuze never sets isFinished.
            _completed.IsFinished = false;
        }

        [Test]
        public void queued_item_should_have_required_properties()
        {
            PrepareClientToReturnQueuedItem();
            var item = Subject.GetItems().Single();
            VerifyQueued(item);
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            PrepareClientToReturnDownloadingItem();
            var item = Subject.GetItems().Single();
            VerifyDownloading(item);
        }

        [Test]
        public void failed_item_should_have_required_properties()
        {
            PrepareClientToReturnFailedItem();
            var item = Subject.GetItems().Single();
            VerifyWarning(item);
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            PrepareClientToReturnCompletedItem(true, ratioLimit: 0.5);
            var item = Subject.GetItems().Single();
            VerifyCompleted(item);

            item.CanBeRemoved.Should().BeTrue();
            item.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void magnet_download_should_not_return_the_item()
        {
            PrepareClientToReturnMagnetItem();
            Subject.GetItems().Count().Should().Be(0);
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
        public void Download_with_MovieDirectory_should_force_directory()
        {
            GivenMovieDirectory();
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();

            Mocker.GetMock<ITransmissionProxy>()
                .Verify(v => v.AddTorrentFromData(It.IsAny<byte[]>(), @"C:/Downloads/Finished/radarr", It.IsAny<TransmissionSettings>()), Times.Once());
        }

        [Test]
        public void Download_with_category_should_force_directory()
        {
            GivenMovieCategory();
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();

            Mocker.GetMock<ITransmissionProxy>()
                  .Verify(v => v.AddTorrentFromData(It.IsAny<byte[]>(), @"C:/Downloads/Finished/transmission/radarr", It.IsAny<TransmissionSettings>()), Times.Once());
        }

        [Test]
        public void Download_with_category_should_not_have_double_slashes()
        {
            GivenMovieCategory();
            GivenSuccessfulDownload();

            _transmissionConfigItems["download-dir"] += "/";

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();

            Mocker.GetMock<ITransmissionProxy>()
                  .Verify(v => v.AddTorrentFromData(It.IsAny<byte[]>(), @"C:/Downloads/Finished/transmission/radarr", It.IsAny<TransmissionSettings>()), Times.Once());
        }

        [Test]
        public void Download_without_MovieDirectory_and_Category_should_use_default()
        {
            GivenSuccessfulDownload();

            var remoteMovie = CreateRemoteMovie();

            var id = Subject.Download(remoteMovie);

            id.Should().NotBeNullOrEmpty();

            Mocker.GetMock<ITransmissionProxy>()
                  .Verify(v => v.AddTorrentFromData(It.IsAny<byte[]>(), null, It.IsAny<TransmissionSettings>()), Times.Once());
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

        [TestCase(TransmissionTorrentStatus.Stopped, DownloadItemStatus.Downloading)]
        [TestCase(TransmissionTorrentStatus.CheckWait, DownloadItemStatus.Downloading)]
        [TestCase(TransmissionTorrentStatus.Check, DownloadItemStatus.Downloading)]
        [TestCase(TransmissionTorrentStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(TransmissionTorrentStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(TransmissionTorrentStatus.SeedingWait, DownloadItemStatus.Downloading)]
        [TestCase(TransmissionTorrentStatus.Seeding, DownloadItemStatus.Downloading)]
        public void GetItems_should_return_queued_item_as_downloadItemStatus(TransmissionTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _queued.Status = apiStatus;

            PrepareClientToReturnQueuedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [TestCase(TransmissionTorrentStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(TransmissionTorrentStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(TransmissionTorrentStatus.Seeding, DownloadItemStatus.Downloading)]
        public void GetItems_should_return_downloading_item_as_downloadItemStatus(TransmissionTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _downloading.Status = apiStatus;

            PrepareClientToReturnDownloadingItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [TestCase(TransmissionTorrentStatus.Stopped, DownloadItemStatus.Completed, false)]
        [TestCase(TransmissionTorrentStatus.CheckWait, DownloadItemStatus.Downloading, false)]
        [TestCase(TransmissionTorrentStatus.Check, DownloadItemStatus.Downloading, false)]
        [TestCase(TransmissionTorrentStatus.Queued, DownloadItemStatus.Queued, false)]
        [TestCase(TransmissionTorrentStatus.SeedingWait, DownloadItemStatus.Completed, false)]
        [TestCase(TransmissionTorrentStatus.Seeding, DownloadItemStatus.Completed, false)]
        public void GetItems_should_return_completed_item_as_downloadItemStatus(TransmissionTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus, bool expectedValue)
        {
            _completed.Status = apiStatus;

            PrepareClientToReturnCompletedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
            item.CanBeRemoved.Should().Be(expectedValue);
            item.CanMoveFiles.Should().Be(expectedValue);
        }

        [Test]
        public void should_return_status_with_outputdirs()
        {
            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"C:\Downloads\Finished\transmission");
        }

        [Test]
        public void should_exclude_items_not_in_category()
        {
            GivenMovieCategory();

            _downloading.DownloadDir = @"C:/Downloads/Finished/transmission/radarr";

            GivenTorrents(new List<TransmissionTorrent>
                {
                    _downloading,
                    _queued
                });

            var items = Subject.GetItems().ToList();

            items.Count.Should().Be(1);
            items.First().Status.Should().Be(DownloadItemStatus.Downloading);
        }

        [Test]
        public void should_exclude_items_not_in_TvDirectory()
        {
            GivenMovieDirectory();

            _downloading.DownloadDir = @"C:/Downloads/Finished/radarr/subdir";

            GivenTorrents(new List<TransmissionTorrent>
                {
                    _downloading,
                    _queued
                });

            var items = Subject.GetItems().ToList();

            items.Count.Should().Be(1);
            items.First().Status.Should().Be(DownloadItemStatus.Downloading);
        }

        [Test]
        public void should_fix_forward_slashes()
        {
            WindowsOnly();

            _downloading.DownloadDir = @"C:/Downloads/Finished/transmission/" + _title;

            GivenTorrents(new List<TransmissionTorrent>
                {
                    _downloading
                });

            var items = Subject.GetItems().ToList();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(@"C:\Downloads\Finished\transmission\" + _title);
        }

        [TestCase(-1)] // Infinite/Unknown
        [TestCase(-2)] // Magnet Downloading
        public void should_ignore_negative_eta(int eta)
        {
            _completed.Eta = eta;

            PrepareClientToReturnCompletedItem();
            var item = Subject.GetItems().Single();
            item.RemainingTime.Should().NotHaveValue();
        }

        [TestCase("14")]
        [TestCase("15")]
        [TestCase("20")]
        public void should_only_check_protocol_version_number(string version)
        {
            Mocker.GetMock<ITransmissionProxy>()
                  .Setup(s => s.GetProtocolVersion(It.IsAny<TransmissionSettings>()))
                  .Returns(version);

            Subject.Test().IsValid.Should().BeTrue();
        }

        [TestCase("")]
        [TestCase("10")]
        [TestCase("foo")]
        public void should_fail_with_unsupported_protocol_version(string version)
        {
            Mocker.GetMock<ITransmissionProxy>()
                  .Setup(s => s.GetProtocolVersion(It.IsAny<TransmissionSettings>()))
                  .Returns(version);

            Subject.Test().IsValid.Should().BeFalse();
        }

        [Test]
        public void should_have_correct_output_directory_for_multifile_torrents()
        {
            WindowsOnly();

            _downloading.DownloadDir = @"C:/Downloads/" + _title;

            GivenTorrents(new List<TransmissionTorrent>
                {
                    _downloading
                });

            var items = Subject.GetItems().ToList();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(@"C:\Downloads\" + _title);
        }

        [Test]
        public void should_have_correct_output_directory_for_singlefile_torrents()
        {
            WindowsOnly();

            var fileName = _title + ".mkv";
            _downloading.Name = fileName;
            _downloading.DownloadDir = @"C:/Downloads";

            GivenTorrents(new List<TransmissionTorrent>
                {
                    _downloading
                });

            var items = Subject.GetItems().ToList();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(@"C:\Downloads\" + fileName);
        }
    }
}
