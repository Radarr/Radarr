using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class UpgradeMediaFileServiceFixture : CoreTest<UpgradeMediaFileService>
    {
        private TrackFile _trackFile;
        private LocalTrack _localTrack;
        private string rootPath = @"C:\Test\Music\Artist".AsOsAgnostic();

        [SetUp]
        public void Setup()
        {
            _localTrack = new LocalTrack();
            _localTrack.Artist = new Artist
                                   {
                                       Path = rootPath
                                   };

            _trackFile = Builder<TrackFile>
                .CreateNew()
                .Build();


            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                 .Setup(c => c.GetParentFolder(It.IsAny<string>()))
                 .Returns<string>(c => Path.GetDirectoryName(c));
        }

        private void GivenSingleTrackWithSingleTrackFile()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Id = 1,
                                                                                    Path = Path.Combine(rootPath, @"Season 01\30.rock.s01e01.avi"),
                                                                                }))
                                                     .Build()
                                                     .ToList();
        }

        private void GivenMultipleTracksWithSingleTrackFile()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Id = 1,
                                                                                    Path = Path.Combine(rootPath, @"Season 01\30.rock.s01e01.avi"),
                                                                                }))
                                                     .Build()
                                                     .ToList();
        }

        private void GivenMultipleTracksWithMultipleTrackFiles()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Id = 1,
                                                                                    Path = Path.Combine(rootPath, @"Season 01\30.rock.s01e01.avi"),
                                                                                }))
                                                     .TheNext(1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Id = 2,
                                                                                    Path = Path.Combine(rootPath, @"Season 01\30.rock.s01e02.avi"),
                                                                                }))
                                                     .Build()
                                                     .ToList();
        }

        [Test]
        public void should_delete_single_track_file_once()
        {
            GivenSingleTrackWithSingleTrackFile();

            Subject.UpgradeTrackFile(_trackFile, _localTrack);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_delete_the_same_track_file_only_once()
        {
            GivenMultipleTracksWithSingleTrackFile();

            Subject.UpgradeTrackFile(_trackFile, _localTrack);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_delete_multiple_different_track_files()
        {
            GivenMultipleTracksWithMultipleTrackFiles();

            Subject.UpgradeTrackFile(_trackFile, _localTrack);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void should_delete_track_file_from_database()
        {
            GivenSingleTrackWithSingleTrackFile();

            Subject.UpgradeTrackFile(_trackFile, _localTrack);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(It.IsAny<TrackFile>(), DeleteMediaFileReason.Upgrade), Times.Once());
        }

        [Test]
        public void should_delete_existing_file_fromdb_if_file_doesnt_exist()
        {
            GivenSingleTrackWithSingleTrackFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeTrackFile(_trackFile, _localTrack);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localTrack.Tracks.Single().TrackFile.Value, DeleteMediaFileReason.Upgrade), Times.Once());
        }

        [Test]
        public void should_not_try_to_recyclebin_existing_file_if_file_doesnt_exist()
        {
            GivenSingleTrackWithSingleTrackFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);

            Subject.UpgradeTrackFile(_trackFile, _localTrack);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_old_track_file_in_oldFiles()
        {
            GivenSingleTrackWithSingleTrackFile();

            Subject.UpgradeTrackFile(_trackFile, _localTrack).OldFiles.Count.Should().Be(1);
        }

        [Test]
        public void should_return_old_track_files_in_oldFiles()
        {
            GivenMultipleTracksWithMultipleTrackFiles();

            Subject.UpgradeTrackFile(_trackFile, _localTrack).OldFiles.Count.Should().Be(2);
        }

        [Test]
        public void should_import_if_existing_file_doesnt_exist_in_db()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(null))
                                                     .Build()
                                                     .ToList();

            Subject.UpgradeTrackFile(_trackFile, _localTrack);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_localTrack.Tracks.Single().TrackFile.Value, It.IsAny<DeleteMediaFileReason>()), Times.Never());
        }
    }
}
