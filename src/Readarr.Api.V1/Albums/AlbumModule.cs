using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Albums
{
    public class AlbumModule : AlbumModuleWithSignalR,
        IHandle<BookGrabbedEvent>,
        IHandle<BookEditedEvent>,
        IHandle<BookUpdatedEvent>,
        IHandle<BookImportedEvent>,
        IHandle<TrackImportedEvent>,
        IHandle<BookFileDeletedEvent>
    {
        protected readonly IAuthorService _authorService;
        protected readonly IAddBookService _addBookService;

        public AlbumModule(IAuthorService authorService,
                           IBookService bookService,
                           IAddBookService addBookService,
                           IAuthorStatisticsService artistStatisticsService,
                           IMapCoversToLocal coverMapper,
                           IUpgradableSpecification upgradableSpecification,
                           IBroadcastSignalRMessage signalRBroadcaster,
                           QualityProfileExistsValidator qualityProfileExistsValidator,
                           MetadataProfileExistsValidator metadataProfileExistsValidator)

        : base(bookService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster)
        {
            _authorService = authorService;
            _addBookService = addBookService;

            GetResourceAll = GetAlbums;
            CreateResource = AddAlbum;
            UpdateResource = UpdateAlbum;
            DeleteResource = DeleteAlbum;
            Put("/monitor", x => SetAlbumsMonitored());

            PostValidator.RuleFor(s => s.ForeignBookId).NotEmpty();
            PostValidator.RuleFor(s => s.Artist.QualityProfileId).SetValidator(qualityProfileExistsValidator);
            PostValidator.RuleFor(s => s.Artist.MetadataProfileId).SetValidator(metadataProfileExistsValidator);
            PostValidator.RuleFor(s => s.Artist.RootFolderPath).IsValidPath().When(s => s.Artist.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Artist.ForeignAuthorId).NotEmpty();
        }

        private List<AlbumResource> GetAlbums()
        {
            var authorIdQuery = Request.Query.AuthorId;
            var bookIdsQuery = Request.Query.BookIds;
            var slugQuery = Request.Query.TitleSlug;
            var includeAllArtistAlbumsQuery = Request.Query.IncludeAllArtistAlbums;

            if (!Request.Query.AuthorId.HasValue && !bookIdsQuery.HasValue && !slugQuery.HasValue)
            {
                var albums = _bookService.GetAllBooks();

                var artists = _authorService.GetAllAuthors().ToDictionary(x => x.AuthorMetadataId);

                foreach (var album in albums)
                {
                    album.Author = artists[album.AuthorMetadataId];
                }

                return MapToResource(albums, false);
            }

            if (authorIdQuery.HasValue)
            {
                int authorId = Convert.ToInt32(authorIdQuery.Value);

                return MapToResource(_bookService.GetBooksByAuthor(authorId), false);
            }

            if (slugQuery.HasValue)
            {
                string titleSlug = slugQuery.Value.ToString();

                var album = _bookService.FindBySlug(titleSlug);

                if (album == null)
                {
                    return MapToResource(new List<Book>(), false);
                }

                if (includeAllArtistAlbumsQuery.HasValue && Convert.ToBoolean(includeAllArtistAlbumsQuery.Value))
                {
                    return MapToResource(_bookService.GetBooksByAuthor(album.AuthorId), false);
                }
                else
                {
                    return MapToResource(new List<Book> { album }, false);
                }
            }

            string bookIdsValue = bookIdsQuery.Value.ToString();

            var bookIds = bookIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => Convert.ToInt32(e))
                                            .ToList();

            return MapToResource(_bookService.GetBooks(bookIds), false);
        }

        private int AddAlbum(AlbumResource albumResource)
        {
            var album = _addBookService.AddBook(albumResource.ToModel());

            return album.Id;
        }

        private void UpdateAlbum(AlbumResource albumResource)
        {
            var album = _bookService.GetBook(albumResource.Id);

            var model = albumResource.ToModel(album);

            _bookService.UpdateBook(model);

            BroadcastResourceChange(ModelAction.Updated, model.Id);
        }

        private void DeleteAlbum(int id)
        {
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");
            var addImportListExclusion = Request.GetBooleanQueryParameter("addImportListExclusion");

            _bookService.DeleteBook(id, deleteFiles, addImportListExclusion);
        }

        private object SetAlbumsMonitored()
        {
            var resource = Request.Body.FromJson<AlbumsMonitoredResource>();

            _bookService.SetMonitored(resource.BookIds, resource.Monitored);

            return ResponseWithCode(MapToResource(_bookService.GetBooks(resource.BookIds), false), HttpStatusCode.Accepted);
        }

        public void Handle(BookGrabbedEvent message)
        {
            foreach (var album in message.Book.Books)
            {
                var resource = album.ToResource();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        public void Handle(BookEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Album, true));
        }

        public void Handle(BookUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Book, true));
        }

        public void Handle(BookDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Book.ToResource());
        }

        public void Handle(BookImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Book, true));
        }

        public void Handle(TrackImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.BookInfo.Book.ToResource());
        }

        public void Handle(BookFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.BookFile.Book.Value, true));
        }
    }
}
