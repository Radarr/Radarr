using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Synology;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class SynologyIndexerFixture : CoreTest<SynologyIndexer>
    {
        private Author _artist;
        private BookDownloadMessage _upgrade;
        private string _rootPath = @"C:\Test\".AsOsAgnostic();

        [SetUp]
        public void SetUp()
        {
            _artist = new Author()
            {
                Path = _rootPath,
            };

            _upgrade = new BookDownloadMessage()
            {
                Author = _artist,

                BookFiles = new List<BookFile>
                {
                    new BookFile
                    {
                        Path = Path.Combine(_rootPath, "file1.S01E01E02.mkv")
                    }
                },

                OldFiles = new List<BookFile>
                {
                    new BookFile
                    {
                        Path = Path.Combine(_rootPath, "file1.S01E01.mkv")
                    },
                    new BookFile
                    {
                        Path = Path.Combine(_rootPath, "file1.S01E02.mkv")
                    }
                }
            };

            Subject.Definition = new NotificationDefinition
            {
                Settings = new SynologyIndexerSettings
                {
                    UpdateLibrary = true
                }
            };
        }

        [Test]
        public void should_not_update_library_if_disabled()
        {
            (Subject.Definition.Settings as SynologyIndexerSettings).UpdateLibrary = false;

            Subject.OnRename(_artist);

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.UpdateFolder(_artist.Path), Times.Never());
        }

        [Test]
        public void should_remove_old_episodes_on_upgrade()
        {
            Subject.OnReleaseImport(_upgrade);

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.DeleteFile(@"C:\Test\file1.S01E01.mkv".AsOsAgnostic()), Times.Once());

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.DeleteFile(@"C:\Test\file1.S01E02.mkv".AsOsAgnostic()), Times.Once());
        }

        [Test]
        public void should_add_new_episode_on_upgrade()
        {
            Subject.OnReleaseImport(_upgrade);

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.AddFile(@"C:\Test\file1.S01E01E02.mkv".AsOsAgnostic()), Times.Once());
        }

        [Test]
        public void should_update_entire_series_folder_on_rename()
        {
            Subject.OnRename(_artist);

            Mocker.GetMock<ISynologyIndexerProxy>()
                .Verify(v => v.UpdateFolder(@"C:\Test\".AsOsAgnostic()), Times.Once());
        }
    }
}
