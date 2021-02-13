using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Synology;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class SynologyIndexerFixture : CoreTest<SynologyIndexer>
    {
        private Movie _movie;
        private DownloadMessage _upgrade;

        [SetUp]
        public void SetUp()
        {
            _movie = new Movie
            {
                Path = @"C:\Test\".AsOsAgnostic()
            };

            _upgrade = new DownloadMessage
            {
                Movie = _movie,

                MovieFile = new MovieFile
                {
                    RelativePath = "moviefile1.mkv"
                },

                OldMovieFiles = new List<MovieFile>
                {
                    new MovieFile
                    {
                        RelativePath = "oldmoviefile1.mkv"
                    },
                    new MovieFile
                    {
                        RelativePath = "oldmoviefile2.mkv"
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

            Subject.OnMovieRename(_movie, new List<RenamedMovieFile>());

            Mocker.GetMock<ISynologyIndexerProxy>()
                  .Verify(v => v.UpdateFolder(_movie.Path), Times.Never());
        }

        [Test]
        public void should_remove_old_movie_on_upgrade()
        {
            Subject.OnDownload(_upgrade);

            Mocker.GetMock<ISynologyIndexerProxy>()
                  .Verify(v => v.DeleteFile(@"C:\Test\oldmoviefile1.mkv".AsOsAgnostic()), Times.Once());

            Mocker.GetMock<ISynologyIndexerProxy>()
                  .Verify(v => v.DeleteFile(@"C:\Test\oldmoviefile2.mkv".AsOsAgnostic()), Times.Once());
        }

        [Test]
        public void should_add_new_movie_on_upgrade()
        {
            Subject.OnDownload(_upgrade);

            Mocker.GetMock<ISynologyIndexerProxy>()
                  .Verify(v => v.AddFile(@"C:\Test\moviefile1.mkv".AsOsAgnostic()), Times.Once());
        }

        [Test]
        public void should_update_entire_movie_folder_on_rename()
        {
            Subject.OnMovieRename(_movie, new List<RenamedMovieFile>());

            Mocker.GetMock<ISynologyIndexerProxy>()
                  .Verify(v => v.UpdateFolder(@"C:\Test\".AsOsAgnostic()), Times.Once());
        }
    }
}
