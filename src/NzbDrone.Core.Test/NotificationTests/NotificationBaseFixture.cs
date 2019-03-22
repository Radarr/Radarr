using System;
using FluentAssertions;
using FluentValidation.Results;
using NUnit.Framework;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Music;
using NzbDrone.Core.Validation;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class NotificationBaseFixture : TestBase
    {
        class TestSetting : IProviderConfig
        {
            public NzbDroneValidationResult Validate()
            {
                return new NzbDroneValidationResult();
            }
        }

        class TestNotificationWithOnReleaseImport : NotificationBase<TestSetting>
        {
            public override string Name => "TestNotification";
            public override string Link => "";


            public override ValidationResult Test()
            {
                throw new NotImplementedException();
            }

            public override void OnReleaseImport(AlbumDownloadMessage message)
            {
                TestLogger.Info("OnDownload was called");
            }

        }

        class TestNotificationWithAllEvents : NotificationBase<TestSetting>
        {
            public override string Name => "TestNotification";
            public override string Link => "";


            public override ValidationResult Test()
            {
                throw new NotImplementedException();
            }

            public override void OnGrab(GrabMessage grabMessage)
            {
                TestLogger.Info("OnGrab was called");
            }

            public override void OnReleaseImport(AlbumDownloadMessage message)
            {
                TestLogger.Info("OnAlbumDownload was called");
            }

            public override void OnRename(Artist artist)
            {
                TestLogger.Info("OnRename was called");
            }

            public override void OnHealthIssue(NzbDrone.Core.HealthCheck.HealthCheck artist)
            {
                TestLogger.Info("OnHealthIssue was called");
            }

            public override void OnDownloadFailure(DownloadFailedMessage message)
            {
                TestLogger.Info("OnDownloadFailure was called");
            }

            public override void OnImportFailure(AlbumDownloadMessage message)
            {
                TestLogger.Info("OnImportFailure was called");
            }

            public override void OnTrackRetag(TrackRetagMessage message)
            {
                TestLogger.Info("OnTrackRetag was called");
            }
        }

        class TestNotificationWithNoEvents : NotificationBase<TestSetting>
        {
            public override string Name => "TestNotification";
            public override string Link => "";


            public override ValidationResult Test()
            {
                throw new NotImplementedException();
            }

          
        }

        [Test]
        public void should_support_OnUpgrade_should_link_to_OnReleaseImport()
        {
            var notification = new TestNotificationWithOnReleaseImport();

            notification.SupportsOnReleaseImport.Should().BeTrue();
            notification.SupportsOnUpgrade.Should().BeTrue();

            notification.SupportsOnGrab.Should().BeFalse();
            notification.SupportsOnRename.Should().BeFalse();
        }

        [Test]
        public void should_support_all_if_implemented()
        {
            var notification = new TestNotificationWithAllEvents();

            notification.SupportsOnGrab.Should().BeTrue();
            notification.SupportsOnReleaseImport.Should().BeTrue();
            notification.SupportsOnUpgrade.Should().BeTrue();
            notification.SupportsOnRename.Should().BeTrue();
            notification.SupportsOnHealthIssue.Should().BeTrue();
            notification.SupportsOnDownloadFailure.Should().BeTrue();
            notification.SupportsOnImportFailure.Should().BeTrue();
            notification.SupportsOnTrackRetag.Should().BeTrue();
        }


        [Test]
        public void should_support_none_if_none_are_implemented()
        {
            var notification = new TestNotificationWithNoEvents();

            notification.SupportsOnGrab.Should().BeFalse();
            notification.SupportsOnReleaseImport.Should().BeFalse();
            notification.SupportsOnUpgrade.Should().BeFalse();
            notification.SupportsOnRename.Should().BeFalse();
            notification.SupportsOnHealthIssue.Should().BeFalse();
            notification.SupportsOnDownloadFailure.Should().BeFalse();
            notification.SupportsOnImportFailure.Should().BeFalse();
            notification.SupportsOnTrackRetag.Should().BeFalse();
        }
    }

}
