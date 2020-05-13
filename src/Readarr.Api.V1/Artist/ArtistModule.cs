using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Artist
{
    public class ArtistModule : ReadarrRestModuleWithSignalR<ArtistResource, NzbDrone.Core.Books.Author>,
                                IHandle<BookImportedEvent>,
                                IHandle<BookEditedEvent>,
                                IHandle<BookFileDeletedEvent>,
                                IHandle<AuthorUpdatedEvent>,
                                IHandle<AuthorEditedEvent>,
                                IHandle<AuthorDeletedEvent>,
                                IHandle<AuthorRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>
    {
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IAddAuthorService _addAuthorService;
        private readonly IAuthorStatisticsService _artistStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRootFolderService _rootFolderService;

        public ArtistModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IAuthorService authorService,
                            IBookService bookService,
                            IAddAuthorService addAuthorService,
                            IAuthorStatisticsService artistStatisticsService,
                            IMapCoversToLocal coverMapper,
                            IManageCommandQueue commandQueueManager,
                            IRootFolderService rootFolderService,
                            RootFolderValidator rootFolderValidator,
                            MappedNetworkDriveValidator mappedNetworkDriveValidator,
                            AuthorPathValidator artistPathValidator,
                            ArtistExistsValidator artistExistsValidator,
                            AuthorAncestorValidator artistAncestorValidator,
                            SystemFolderValidator systemFolderValidator,
                            QualityProfileExistsValidator qualityProfileExistsValidator,
                            MetadataProfileExistsValidator metadataProfileExistsValidator)
            : base(signalRBroadcaster)
        {
            _authorService = authorService;
            _bookService = bookService;
            _addAuthorService = addAuthorService;
            _artistStatisticsService = artistStatisticsService;

            _coverMapper = coverMapper;
            _commandQueueManager = commandQueueManager;
            _rootFolderService = rootFolderService;

            GetResourceAll = AllArtists;
            GetResourceById = GetArtist;
            CreateResource = AddArtist;
            UpdateResource = UpdateArtist;
            DeleteResource = DeleteArtist;

            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.QualityProfileId));
            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.MetadataProfileId));

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(artistPathValidator)
                           .SetValidator(artistAncestorValidator)
                           .SetValidator(systemFolderValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(qualityProfileExistsValidator);
            SharedValidator.RuleFor(s => s.MetadataProfileId).SetValidator(metadataProfileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.ArtistName).NotEmpty();
            PostValidator.RuleFor(s => s.ForeignAuthorId).NotEmpty().SetValidator(artistExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private ArtistResource GetArtist(int id)
        {
            var artist = _authorService.GetAuthor(id);
            return GetArtistResource(artist);
        }

        private ArtistResource GetArtistResource(NzbDrone.Core.Books.Author artist)
        {
            if (artist == null)
            {
                return null;
            }

            var resource = artist.ToResource();
            MapCoversToLocal(resource);
            FetchAndLinkArtistStatistics(resource);
            LinkNextPreviousAlbums(resource);

            //PopulateAlternateTitles(resource);
            LinkRootFolderPath(resource);

            return resource;
        }

        private List<ArtistResource> AllArtists()
        {
            var artistStats = _artistStatisticsService.AuthorStatistics();
            var artistsResources = _authorService.GetAllAuthors().ToResource();

            MapCoversToLocal(artistsResources.ToArray());
            LinkNextPreviousAlbums(artistsResources.ToArray());
            LinkArtistStatistics(artistsResources, artistStats);

            //PopulateAlternateTitles(seriesResources);
            return artistsResources;
        }

        private int AddArtist(ArtistResource artistResource)
        {
            var artist = _addAuthorService.AddAuthor(artistResource.ToModel());

            return artist.Id;
        }

        private void UpdateArtist(ArtistResource artistResource)
        {
            var moveFiles = Request.GetBooleanQueryParameter("moveFiles");
            var artist = _authorService.GetAuthor(artistResource.Id);

            if (moveFiles)
            {
                var sourcePath = artist.Path;
                var destinationPath = artistResource.Path;

                _commandQueueManager.Push(new MoveAuthorCommand
                {
                    AuthorId = artist.Id,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Trigger = CommandTrigger.Manual
                });
            }

            var model = artistResource.ToModel(artist);

            _authorService.UpdateAuthor(model);

            BroadcastResourceChange(ModelAction.Updated, artistResource);
        }

        private void DeleteArtist(int id)
        {
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");
            var addImportListExclusion = Request.GetBooleanQueryParameter("addImportListExclusion");

            _authorService.DeleteAuthor(id, deleteFiles, addImportListExclusion);
        }

        private void MapCoversToLocal(params ArtistResource[] artists)
        {
            foreach (var artistResource in artists)
            {
                _coverMapper.ConvertToLocalUrls(artistResource.Id, MediaCoverEntity.Author, artistResource.Images);
            }
        }

        private void LinkNextPreviousAlbums(params ArtistResource[] artists)
        {
            var nextAlbums = _bookService.GetNextBooksByAuthorMetadataId(artists.Select(x => x.ArtistMetadataId));
            var lastAlbums = _bookService.GetLastBooksByAuthorMetadataId(artists.Select(x => x.ArtistMetadataId));

            foreach (var artistResource in artists)
            {
                artistResource.NextAlbum = nextAlbums.FirstOrDefault(x => x.AuthorMetadataId == artistResource.ArtistMetadataId);
                artistResource.LastAlbum = lastAlbums.FirstOrDefault(x => x.AuthorMetadataId == artistResource.ArtistMetadataId);
            }
        }

        private void FetchAndLinkArtistStatistics(ArtistResource resource)
        {
            LinkArtistStatistics(resource, _artistStatisticsService.AuthorStatistics(resource.Id));
        }

        private void LinkArtistStatistics(List<ArtistResource> resources, List<AuthorStatistics> artistStatistics)
        {
            foreach (var artist in resources)
            {
                var stats = artistStatistics.SingleOrDefault(ss => ss.AuthorId == artist.Id);
                if (stats == null)
                {
                    continue;
                }

                LinkArtistStatistics(artist, stats);
            }
        }

        private void LinkArtistStatistics(ArtistResource resource, AuthorStatistics artistStatistics)
        {
            resource.Statistics = artistStatistics.ToResource();
        }

        //private void PopulateAlternateTitles(List<ArtistResource> resources)
        //{
        //    foreach (var resource in resources)
        //    {
        //        PopulateAlternateTitles(resource);
        //    }
        //}

        //private void PopulateAlternateTitles(ArtistResource resource)
        //{
        //    var mappings = _sceneMappingService.FindByTvdbId(resource.TvdbId);

        //    if (mappings == null) return;

        //    resource.AlternateTitles = mappings.Select(v => new AlternateTitleResource { Title = v.Title, SeasonNumber = v.SeasonNumber, SceneSeasonNumber = v.SceneSeasonNumber }).ToList();
        //}
        private void LinkRootFolderPath(ArtistResource resource)
        {
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
        }

        public void Handle(BookImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Author));
        }

        public void Handle(BookEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Album.Author.Value));
        }

        public void Handle(BookFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.BookFile.Author.Value));
        }

        public void Handle(AuthorUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Author));
        }

        public void Handle(AuthorEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Author));
        }

        public void Handle(AuthorDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Author.ToResource());
        }

        public void Handle(AuthorRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Author.Id);
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Author));
        }
    }
}
