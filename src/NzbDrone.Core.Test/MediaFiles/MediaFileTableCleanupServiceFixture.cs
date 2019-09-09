using System.Collections.Generic;
using System.Linq;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class MediaFileTableCleanupServiceFixture : CoreTest<MediaFileTableCleanupService>
    {
        private readonly string DELETED_PATH = @"c:\ANY FILE STARTING WITH THIS PATH IS CONSIDERED DELETED!".AsOsAgnostic();
        private List<Track> _tracks;
        private Artist _artist;

        [SetUp]
        public void SetUp()
        {
            _tracks = Builder<Track>.CreateListOfSize(10)
                  .Build()
                  .ToList();

            _artist = Builder<Artist>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\Music\Artist".AsOsAgnostic())
                                     .Build();

            Mocker.GetMock<ITrackService>()
                  .Setup(c => c.GetTracksByFileId(It.IsAny<IEnumerable<int>>()))
                  .Returns((IEnumerable<int> ids) => _tracks.Where(y => ids.Contains(y.TrackFileId)).ToList());
        }

        private void GivenTrackFiles(IEnumerable<TrackFile> trackFiles)
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesWithBasePath(It.IsAny<string>()))
                  .Returns(trackFiles.ToList());
        }

        private void GivenFilesAreNotAttachedToTrack()
        {
            Mocker.GetMock<ITrackService>()
                  .Setup(c => c.GetTracksByFileId(It.IsAny<int>()))
                  .Returns(new List<Track>());
        }

        private List<string> FilesOnDisk(IEnumerable<TrackFile> trackFiles)
        {
            return trackFiles.Select(e => e.Path).ToList();
        }

        [Test]
        public void should_skip_files_that_exist_on_disk()
        {
            var trackFiles = Builder<TrackFile>.CreateListOfSize(10)
                .All()
                .With(x => x.Path = Path.Combine(@"c:\test".AsOsAgnostic(), Path.GetRandomFileName()))
                .Build();

            GivenTrackFiles(trackFiles);

            Subject.Clean(_artist, FilesOnDisk(trackFiles));

            Mocker.GetMock<IMediaFileService>()
                .Verify(c => c.DeleteMany(It.Is<List<TrackFile>>(x => x.Count == 0), DeleteMediaFileReason.MissingFromDisk), Times.Once());
        }

        [Test]
        public void should_delete_non_existent_files()
        {
            var trackFiles = Builder<TrackFile>.CreateListOfSize(10)
                .All()
                .With(x => x.Path = Path.Combine(@"c:\test".AsOsAgnostic(), Path.GetRandomFileName()))
                .Random(2)
                .With(c => c.Path = Path.Combine(DELETED_PATH, Path.GetRandomFileName()))
                .Build();

            GivenTrackFiles(trackFiles);

            Subject.Clean(_artist, FilesOnDisk(trackFiles.Where(e => !e.Path.StartsWith(DELETED_PATH))));

            Mocker.GetMock<IMediaFileService>()
                .Verify(c => c.DeleteMany(It.Is<List<TrackFile>>(e => e.Count == 2 && e.All(y => y.Path.StartsWith(DELETED_PATH))), DeleteMediaFileReason.MissingFromDisk), Times.Once());
        }

        [Test]
        public void should_unlink_track_when_trackFile_does_not_exist()
        {
            var trackFiles = Builder<TrackFile>.CreateListOfSize(10)
                .Random(10)
                .With(c => c.Path = Path.Combine(@"c:\test".AsOsAgnostic(), Path.GetRandomFileName()))
                .Build();

            GivenTrackFiles(trackFiles);

            Subject.Clean(_artist, new List<string>());

            Mocker.GetMock<ITrackService>()
                .Verify(c => c.SetFileIds(It.Is<List<Track>>(e => e.Count == 10 && e.All(y => y.TrackFileId == 0))), Times.Once());
        }

        [Test]
        public void should_not_update_track_when_trackFile_exists()
        {
            var trackFiles = Builder<TrackFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.Path = Path.Combine(@"c:\test".AsOsAgnostic(), Path.GetRandomFileName()))
                                .Build();

            GivenTrackFiles(trackFiles);

            Subject.Clean(_artist, FilesOnDisk(trackFiles));

            Mocker.GetMock<ITrackService>().Verify(c => c.SetFileIds(It.Is<List<Track>>(x => x.Count == 0)), Times.Once());
        }
    }
}
