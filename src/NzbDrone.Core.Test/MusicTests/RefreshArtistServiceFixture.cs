using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Test.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.History;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshArtistServiceFixture : CoreTest<RefreshArtistService>
    {
        private Artist _artist;
        private Album _album1;
        private Album _album2;
        private List<Album> _albums;

        [SetUp]
        public void Setup()
        {
            _album1 = Builder<Album>.CreateNew()
                .With(s => s.ForeignAlbumId = "1")
                .Build();

            _album2 = Builder<Album>.CreateNew()
                .With(s => s.ForeignAlbumId = "2")
                .Build();

            _albums = new List<Album> {_album1, _album2};

            var metadata = Builder<ArtistMetadata>.CreateNew().Build();

            _artist = Builder<Artist>.CreateNew()
                .With(a => a.Metadata = metadata)
                .Build();

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                  .Setup(s => s.GetArtist(_artist.Id))
                  .Returns(_artist);

            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .Setup(s => s.InsertMany(It.IsAny<List<Album>>()));

            Mocker.GetMock<IProvideArtistInfo>()
                  .Setup(s => s.GetArtistInfo(It.IsAny<string>(), It.IsAny<int>()))
                  .Callback(() => { throw new ArtistNotFoundException(_artist.ForeignArtistId); });

            Mocker.GetMock<IMediaFileService>()
                .Setup(x => x.GetFilesByArtist(It.IsAny<int>()))
                .Returns(new List<TrackFile>());

            Mocker.GetMock<IHistoryService>()
                .Setup(x => x.GetByArtist(It.IsAny<int>(), It.IsAny<HistoryEventType?>()))
                .Returns(new List<History.History>());
        }

        private void GivenNewArtistInfo(Artist artist)
        {
            Mocker.GetMock<IProvideArtistInfo>()
                  .Setup(s => s.GetArtistInfo(_artist.ForeignArtistId, _artist.MetadataProfileId))
                  .Returns(artist);
        }
        
        private void GivenArtistFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(x => x.GetFilesByArtist(It.IsAny<int>()))
                  .Returns(Builder<TrackFile>.CreateListOfSize(1).BuildList());
        }

        private void GivenAlbumsForRefresh()
        {
            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .Setup(s => s.GetAlbumsForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new List<Album>());
        }

        private void AllowArtistUpdate()
        {
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .Setup(x => x.UpdateArtist(It.IsAny<Artist>()))
                .Returns((Artist a) => a);
        }

        [Test]
        public void should_not_publish_artist_updated_event_if_metadata_not_updated()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Metadata = _artist.Metadata.Value.JsonClone();
            newArtistInfo.Albums = _albums;

            GivenNewArtistInfo(newArtistInfo);
            GivenAlbumsForRefresh();
            AllowArtistUpdate();

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            VerifyEventNotPublished<ArtistUpdatedEvent>();
            VerifyEventPublished<ArtistRefreshCompleteEvent>();
        }

        [Test]
        public void should_publish_artist_updated_event_if_metadata_updated()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Metadata = _artist.Metadata.Value.JsonClone();
            newArtistInfo.Metadata.Value.Images = new List<MediaCover.MediaCover> {
                new MediaCover.MediaCover(MediaCover.MediaCoverTypes.Logo, "dummy")
            };
            newArtistInfo.Albums = _albums;

            GivenNewArtistInfo(newArtistInfo);
            GivenAlbumsForRefresh();
            AllowArtistUpdate();

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            VerifyEventPublished<ArtistUpdatedEvent>();
            VerifyEventPublished<ArtistRefreshCompleteEvent>();
        }

        [Test]
        public void should_log_error_and_delete_if_musicbrainz_id_not_found_and_artist_has_no_files()
        {
            Mocker.GetMock<IArtistService>()
                .Setup(x => x.DeleteArtist(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()));

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.IsAny<Artist>()), Times.Never());
            
            Mocker.GetMock<IArtistService>()
                .Verify(v => v.DeleteArtist(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
            ExceptionVerification.ExpectedWarns(1);
        }
        
        [Test]
        public void should_log_error_but_not_delete_if_musicbrainz_id_not_found_and_artist_has_files()
        {
            GivenArtistFiles();
            GivenAlbumsForRefresh();
            
            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.IsAny<Artist>()), Times.Never());
            
            Mocker.GetMock<IArtistService>()
                .Verify(v => v.DeleteArtist(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed_and_no_clash()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Metadata = _artist.Metadata.Value.JsonClone();
            newArtistInfo.Albums = _albums;
            newArtistInfo.ForeignArtistId = _artist.ForeignArtistId + 1;
            newArtistInfo.Metadata.Value.Id = 100;

            GivenNewArtistInfo(newArtistInfo);

            var seq = new MockSequence();

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .Setup(x => x.FindById(newArtistInfo.ForeignArtistId))
                .Returns(default(Artist));

            // Make sure that the artist is updated before we refresh the albums
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateArtist(It.IsAny<Artist>()))
                .Returns((Artist a) => a);

            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetAlbumsForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new List<Album>());

            // Update called twice for a move/merge
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateArtist(It.IsAny<Artist>()))
                .Returns((Artist a) => a);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.ArtistMetadataId == 100 && s.ForeignArtistId == newArtistInfo.ForeignArtistId)),
                        Times.Exactly(2));
        }

        [Test]
        public void should_merge_if_musicbrainz_id_changed_and_new_id_already_exists()
        {
            var existing = _artist;

            var clash = _artist.JsonClone();
            clash.Id = 100;
            clash.Metadata = existing.Metadata.Value.JsonClone();
            clash.Metadata.Value.Id = 101;
            clash.Metadata.Value.ForeignArtistId = clash.Metadata.Value.ForeignArtistId + 1;

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .Setup(x => x.FindById(clash.Metadata.Value.ForeignArtistId))
                .Returns(clash);

            var newArtistInfo = clash.JsonClone();
            newArtistInfo.Metadata = clash.Metadata.Value.JsonClone();
            newArtistInfo.Albums = _albums.JsonClone();
            newArtistInfo.Albums.Value.ForEach(x => x.Id = 0);

            GivenNewArtistInfo(newArtistInfo);

            var seq = new MockSequence();

            // Make sure that the artist is updated before we refresh the albums
            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetAlbumsByArtist(existing.Id))
                .Returns(_albums);

            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateMany(It.IsAny<List<Album>>()));

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.DeleteArtist(existing.Id, It.IsAny<bool>(), false));
            
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateArtist(It.Is<Artist>(a => a.Id == clash.Id)))
                .Returns((Artist a) => a);

            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetAlbumsForRefresh(clash.ArtistMetadataId, It.IsAny<IEnumerable<string>>()))
                .Returns(_albums);

            // Update called twice for a move/merge
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateArtist(It.IsAny<Artist>()))
                .Returns((Artist a) => a);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            // the retained artist gets updated
            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.Id == clash.Id)), Times.Exactly(2));

            // the old one gets removed
            Mocker.GetMock<IArtistService>()
                .Verify(v => v.DeleteArtist(existing.Id, false, false));

            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Album>>(x => x.Count == _albums.Count)));

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
