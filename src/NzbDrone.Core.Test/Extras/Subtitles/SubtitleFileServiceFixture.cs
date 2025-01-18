using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Subtitles
{
    [TestFixture]
    public class SubtitleFileServiceFixture : CoreTest<SubtitleFileService>
    {
        private MovieFile _movieFile;

        [SetUp]
        public void Setup()
        {
            var movieFolder = @"C:\Test\Movies\Movie Title".AsOsAgnostic();

            var movie = Builder<Movie>.CreateNew()
                .With(s => s.Path = movieFolder)
                .Build();

            _movieFile = Builder<MovieFile>.CreateNew()
                .With(f => f.Path = Path.Combine(movie.Path, "Movie Title - 2022.mkv").AsOsAgnostic())
                .With(f => f.RelativePath = @"Movie Title - 2022.mkv")
                .Build();

            var extra = Builder<SubtitleFile>.CreateNew()
                .With(f => f.RelativePath = @"Movie Title - 2022.extra")
                .Build();

            Mocker.GetMock<IMovieService>()
                .Setup(s => s.GetMovie(_movieFile.Id))
                .Returns(movie);

            Mocker.GetMock<IExtraFileRepository<SubtitleFile>>()
                .Setup(s => s.GetFilesByMovieFile(It.IsAny<int>()))
                .Returns(new List<SubtitleFile>() { extra });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FileExists(It.IsAny<string>()))
                .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetParentFolder(It.IsAny<string>()))
                .Returns(movieFolder);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetParentFolder(movieFolder))
                .Returns(@"C:\Test\Movies".AsOsAgnostic());
        }

        [Test]
        public void cleanup_depends_on_reason_and_config(
            [Values] DeleteMediaFileReason reason,
            [Values(false, true)] bool keep)
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.UpgradeKeepSubtitlesFiles)
                .Returns(keep);

            var evt = new MovieFileDeletedEvent(_movieFile, reason);

            Subject.HandleAsync(evt);

            switch (reason)
            {
                case DeleteMediaFileReason.NoLinkedEpisodes:
                    {
                        Mocker.GetMock<IRecycleBinProvider>().Verify(
                            v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()),
                            Times.Never);
                        Mocker.GetMock<IExtraFileRepository<SubtitleFile>>().Verify(
                            v => v.DeleteForMovieFile(It.IsAny<int>()),
                            Times.Once);
                        break;
                    }

                case DeleteMediaFileReason.Upgrade:
                    {
                        Func<Times> times = keep ? Times.Never : Times.Once;
                        Mocker.GetMock<IRecycleBinProvider>().Verify(
                            v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()),
                            times);
                        Mocker.GetMock<IExtraFileRepository<SubtitleFile>>().Verify(
                            v => v.DeleteForMovieFile(It.IsAny<int>()),
                            times);
                        break;
                    }

                default:
                    {
                        Mocker.GetMock<IRecycleBinProvider>().Verify(
                            v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()),
                            Times.Once);
                        Mocker.GetMock<IExtraFileRepository<SubtitleFile>>().Verify(
                            v => v.DeleteForMovieFile(It.IsAny<int>()),
                            Times.Once);
                        break;
                    }
            }
        }
    }
}
