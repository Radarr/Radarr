using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Tdarr;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Tdarr
{
    [TestFixture]
    public class TdarrFixture : CoreTest<Notifications.Tdarr.Tdarr>
    {
        private TdarrSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = new TdarrSettings
            {
                Host = "localhost",
                LibraryId = "test-library"
            };

            Subject.Definition = new NotificationDefinition
            {
                Settings = _settings
            };
        }

        [Test]
        public void should_scan_imported_movie_file()
        {
            Subject.OnDownload(new DownloadMessage
            {
                Movie = new Movie { Path = @"/movies/Test Movie (2026)" },
                MovieFile = new MovieFile { RelativePath = "Test Movie (2026).mkv" }
            });

            Mocker.GetMock<ITdarrProxy>()
                  .Verify(v => v.ScanFile("/movies/Test Movie (2026)/Test Movie (2026).mkv", _settings), Times.Once());
        }

        [Test]
        public void should_map_imported_movie_file_path()
        {
            _settings.MapFrom = "/movies";
            _settings.MapTo = "/media/movies";

            Subject.OnDownload(new DownloadMessage
            {
                Movie = new Movie { Path = @"/movies/Test Movie (2026)" },
                MovieFile = new MovieFile { RelativePath = "Test Movie (2026).mkv" }
            });

            Mocker.GetMock<ITdarrProxy>()
                  .Verify(v => v.ScanFile("/media/movies/Test Movie (2026)/Test Movie (2026).mkv", _settings), Times.Once());
        }

        [Test]
        public void should_scan_renamed_movie_files()
        {
            var movie = new Movie { Path = @"/movies/Test Movie (2026)" };
            var renamedFiles = new List<RenamedMovieFile>
            {
                new RenamedMovieFile
                {
                    MovieFile = new MovieFile { RelativePath = "Test Movie (2026) Bluray-1080p.mkv" }
                }
            };

            Subject.OnMovieRename(movie, renamedFiles);

            Mocker.GetMock<ITdarrProxy>()
                  .Verify(v => v.ScanFile("/movies/Test Movie (2026)/Test Movie (2026) Bluray-1080p.mkv", _settings), Times.Once());
        }
    }
}
