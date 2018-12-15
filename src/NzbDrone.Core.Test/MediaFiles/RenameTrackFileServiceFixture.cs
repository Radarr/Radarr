using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class RenameTrackFileServiceFixture : CoreTest<RenameTrackFileService>
    {
        private Artist _artist;
        private List<TrackFile> _trackFiles;
            
        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                     .Build();

            _trackFiles = Builder<TrackFile>.CreateListOfSize(2)
                                                .All()
                                                .With(e => e.Artist = _artist)
                                                .Build()
                                                .ToList();

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(_artist.Id))
                  .Returns(_artist);
        }

        private void GivenNoTrackFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<TrackFile>());
        }

        private void GivenTrackFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(_trackFiles);
        }

        private void GivenMovedFiles()
        {
            Mocker.GetMock<IMoveTrackFiles>()
                  .Setup(s => s.MoveTrackFile(It.IsAny<TrackFile>(), _artist));
        }

        [Test]
        public void should_not_publish_event_if_no_files_to_rename()
        {
            GivenNoTrackFiles();

            Subject.Execute(new RenameFilesCommand(_artist.Id, new List<int>{1}));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<ArtistRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_not_publish_event_if_no_files_are_renamed()
        {
            GivenTrackFiles();

            Mocker.GetMock<IMoveTrackFiles>()
                  .Setup(s => s.MoveTrackFile(It.IsAny<TrackFile>(), It.IsAny<Artist>()))
                  .Throws(new SameFilenameException("Same file name", "Filename"));

            Subject.Execute(new RenameFilesCommand(_artist.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<ArtistRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_publish_event_if_files_are_renamed()
        {
            GivenTrackFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_artist.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<ArtistRenamedEvent>()), Times.Once());
        }

        [Test]
        public void should_update_moved_files()
        {
            GivenTrackFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_artist.Id, new List<int> { 1 }));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<TrackFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_get_trackfiles_by_ids_only()
        {
            GivenTrackFiles();
            GivenMovedFiles();

            var files = new List<int> { 1 };

            Subject.Execute(new RenameFilesCommand(_artist.Id, files));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Get(files), Times.Once());
        }
    }
}
