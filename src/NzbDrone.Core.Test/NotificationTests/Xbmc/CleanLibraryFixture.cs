using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class CleanLibraryFixture : CoreTest<XbmcService>
    {
        private XbmcSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new XbmcSettings
            {
                Host = "localhost",
                Port = 8080
            };

            Subject.ScanWaitAttempts = 3;
            Subject.ScanWaitInterval = TimeSpan.Zero;

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetActivePlayers(_settings))
                  .Returns(new List<ActivePlayer>());
        }

        [Test]
        public void should_clean_when_kodi_is_not_scanning()
        {
            Subject.Clean(_settings);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.CleanLibrary(_settings), Times.Once());
        }

        [Test]
        public void should_wait_for_scan_to_finish_before_cleaning()
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .SetupSequence(s => s.IsScanningVideoLibrary(_settings))
                  .Returns(true)
                  .Returns(true)
                  .Returns(false);

            Subject.Clean(_settings);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.IsScanningVideoLibrary(_settings), Times.Exactly(3));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.CleanLibrary(_settings), Times.Once());
        }

        [Test]
        public void should_clean_when_scan_check_fails()
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.IsScanningVideoLibrary(_settings))
                  .Throws(new XbmcJsonException("Invalid params."));

            Subject.Clean(_settings);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.CleanLibrary(_settings), Times.Once());
        }

        [Test]
        public void should_not_clean_when_video_starts_playing_while_waiting()
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .SetupSequence(s => s.IsScanningVideoLibrary(_settings))
                  .Returns(true)
                  .Returns(false);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .SetupSequence(s => s.GetActivePlayers(_settings))
                  .Returns(new List<ActivePlayer>())
                  .Returns(new List<ActivePlayer> { new ActivePlayer(1, "video") });

            Subject.Clean(_settings);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.CleanLibrary(_settings), Times.Never());
        }

        [Test]
        public void should_not_clean_while_kodi_keeps_scanning()
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.IsScanningVideoLibrary(_settings))
                  .Returns(true);

            Subject.Clean(_settings);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.IsScanningVideoLibrary(_settings), Times.Exactly(3));

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.CleanLibrary(_settings), Times.Never());
        }

        [Test]
        public void should_not_check_whether_kodi_is_scanning_when_video_is_playing()
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetActivePlayers(_settings))
                  .Returns(new List<ActivePlayer> { new ActivePlayer(1, "video") });

            Subject.Clean(_settings);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.IsScanningVideoLibrary(_settings), Times.Never());

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.CleanLibrary(_settings), Times.Never());
        }
    }
}
