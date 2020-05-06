using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.History;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshArtistServiceFixture : CoreTest<RefreshArtistService>
    {
        private Author _artist;
        private Book _album1;
        private Book _album2;
        private List<Book> _albums;
        private List<Book> _remoteAlbums;

        [SetUp]
        public void Setup()
        {
            _album1 = Builder<Book>.CreateNew()
                .With(s => s.ForeignBookId = "1")
                .Build();

            _album2 = Builder<Book>.CreateNew()
                .With(s => s.ForeignBookId = "2")
                .Build();

            _albums = new List<Book> { _album1, _album2 };

            _remoteAlbums = _albums.JsonClone();
            _remoteAlbums.ForEach(x => x.Id = 0);

            var metadata = Builder<AuthorMetadata>.CreateNew().Build();
            var series = Builder<Series>.CreateListOfSize(1).BuildList();

            _artist = Builder<Author>.CreateNew()
                .With(a => a.Metadata = metadata)
                .With(a => a.Series = series)
                .Build();

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                  .Setup(s => s.GetArtists(new List<int> { _artist.Id }))
                  .Returns(new List<Author> { _artist });

            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .Setup(s => s.InsertMany(It.IsAny<List<Book>>()));

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.FilterBooks(It.IsAny<Author>(), It.IsAny<int>()))
                .Returns(_albums);

            Mocker.GetMock<IProvideAuthorInfo>()
                  .Setup(s => s.GetAuthorInfo(It.IsAny<string>()))
                  .Callback(() => { throw new ArtistNotFoundException(_artist.ForeignAuthorId); });

            Mocker.GetMock<IMediaFileService>()
                .Setup(x => x.GetFilesByArtist(It.IsAny<int>()))
                .Returns(new List<BookFile>());

            Mocker.GetMock<IHistoryService>()
                .Setup(x => x.GetByArtist(It.IsAny<int>(), It.IsAny<HistoryEventType?>()))
                .Returns(new List<History.History>());

            Mocker.GetMock<IImportListExclusionService>()
                .Setup(x => x.FindByForeignId(It.IsAny<List<string>>()))
                .Returns(new List<ImportListExclusion>());

            Mocker.GetMock<IRootFolderService>()
                .Setup(x => x.All())
                .Returns(new List<RootFolder>());
        }

        private void GivenNewArtistInfo(Author artist)
        {
            Mocker.GetMock<IProvideAuthorInfo>()
                  .Setup(s => s.GetAuthorInfo(_artist.ForeignAuthorId))
                  .Returns(artist);
        }

        private void GivenArtistFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(x => x.GetFilesByArtist(It.IsAny<int>()))
                  .Returns(Builder<BookFile>.CreateListOfSize(1).BuildList());
        }

        private void GivenAlbumsForRefresh(List<Book> albums)
        {
            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .Setup(s => s.GetAlbumsForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(albums);
        }

        private void AllowArtistUpdate()
        {
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .Setup(x => x.UpdateArtist(It.IsAny<Author>()))
                .Returns((Author a) => a);
        }

        [Test]
        public void should_not_publish_artist_updated_event_if_metadata_not_updated()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Metadata = _artist.Metadata.Value.JsonClone();
            newArtistInfo.Books = _remoteAlbums;

            GivenNewArtistInfo(newArtistInfo);
            GivenAlbumsForRefresh(_albums);
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
            newArtistInfo.Metadata.Value.Images = new List<MediaCover.MediaCover>
            {
                new MediaCover.MediaCover(MediaCover.MediaCoverTypes.Logo, "dummy")
            };
            newArtistInfo.Books = _remoteAlbums;

            GivenNewArtistInfo(newArtistInfo);
            GivenAlbumsForRefresh(new List<Book>());
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
                .Verify(v => v.UpdateArtist(It.IsAny<Author>()), Times.Never());

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.DeleteArtist(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_log_error_but_not_delete_if_musicbrainz_id_not_found_and_artist_has_files()
        {
            GivenArtistFiles();
            GivenAlbumsForRefresh(new List<Book>());

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.IsAny<Author>()), Times.Never());

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.DeleteArtist(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void should_update_if_musicbrainz_id_changed_and_no_clash()
        {
            var newArtistInfo = _artist.JsonClone();
            newArtistInfo.Metadata = _artist.Metadata.Value.JsonClone();
            newArtistInfo.Books = _remoteAlbums;
            newArtistInfo.ForeignAuthorId = _artist.ForeignAuthorId + 1;
            newArtistInfo.Metadata.Value.Id = 100;

            GivenNewArtistInfo(newArtistInfo);

            var seq = new MockSequence();

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .Setup(x => x.FindById(newArtistInfo.ForeignAuthorId))
                .Returns(default(Author));

            // Make sure that the artist is updated before we refresh the albums
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateArtist(It.IsAny<Author>()))
                .Returns((Author a) => a);

            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetAlbumsForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new List<Book>());

            // Update called twice for a move/merge
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateArtist(It.IsAny<Author>()))
                .Returns((Author a) => a);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.Is<Author>(s => s.AuthorMetadataId == 100 && s.ForeignAuthorId == newArtistInfo.ForeignAuthorId)),
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
            clash.Metadata.Value.ForeignAuthorId = clash.Metadata.Value.ForeignAuthorId + 1;

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .Setup(x => x.FindById(clash.Metadata.Value.ForeignAuthorId))
                .Returns(clash);

            var newArtistInfo = clash.JsonClone();
            newArtistInfo.Metadata = clash.Metadata.Value.JsonClone();
            newArtistInfo.Books = _remoteAlbums;

            GivenNewArtistInfo(newArtistInfo);

            var seq = new MockSequence();

            // Make sure that the artist is updated before we refresh the albums
            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetAlbumsByArtist(existing.Id))
                .Returns(_albums);

            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateMany(It.IsAny<List<Book>>()));

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.DeleteArtist(existing.Id, It.IsAny<bool>(), false));

            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateArtist(It.Is<Author>(a => a.Id == clash.Id)))
                .Returns((Author a) => a);

            Mocker.GetMock<IAlbumService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.GetAlbumsForRefresh(clash.AuthorMetadataId, It.IsAny<IEnumerable<string>>()))
                .Returns(_albums);

            // Update called twice for a move/merge
            Mocker.GetMock<IArtistService>(MockBehavior.Strict)
                .InSequence(seq)
                .Setup(x => x.UpdateArtist(It.IsAny<Author>()))
                .Returns((Author a) => a);

            Subject.Execute(new RefreshArtistCommand(_artist.Id));

            // the retained artist gets updated
            Mocker.GetMock<IArtistService>()
                .Verify(v => v.UpdateArtist(It.Is<Author>(s => s.Id == clash.Id)), Times.Exactly(2));

            // the old one gets removed
            Mocker.GetMock<IArtistService>()
                .Verify(v => v.DeleteArtist(existing.Id, false, false));

            Mocker.GetMock<IAlbumService>()
                .Verify(v => v.UpdateMany(It.Is<List<Book>>(x => x.Count == _albums.Count)));

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
