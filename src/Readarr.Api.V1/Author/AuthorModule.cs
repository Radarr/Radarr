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

namespace Readarr.Api.V1.Author
{
    public class AuthorModule : ReadarrRestModuleWithSignalR<AuthorResource, NzbDrone.Core.Books.Author>,
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
        private readonly IAuthorStatisticsService _authorStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRootFolderService _rootFolderService;

        public AuthorModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IAuthorService authorService,
                            IBookService bookService,
                            IAddAuthorService addAuthorService,
                            IAuthorStatisticsService authorStatisticsService,
                            IMapCoversToLocal coverMapper,
                            IManageCommandQueue commandQueueManager,
                            IRootFolderService rootFolderService,
                            RootFolderValidator rootFolderValidator,
                            MappedNetworkDriveValidator mappedNetworkDriveValidator,
                            AuthorPathValidator authorPathValidator,
                            AuthorExistsValidator authorExistsValidator,
                            AuthorAncestorValidator authorAncestorValidator,
                            SystemFolderValidator systemFolderValidator,
                            QualityProfileExistsValidator qualityProfileExistsValidator,
                            MetadataProfileExistsValidator metadataProfileExistsValidator)
            : base(signalRBroadcaster)
        {
            _authorService = authorService;
            _bookService = bookService;
            _addAuthorService = addAuthorService;
            _authorStatisticsService = authorStatisticsService;

            _coverMapper = coverMapper;
            _commandQueueManager = commandQueueManager;
            _rootFolderService = rootFolderService;

            GetResourceAll = AllAuthors;
            GetResourceById = GetAuthor;
            CreateResource = AddAuthor;
            UpdateResource = UpdateAuthor;
            DeleteResource = DeleteAuthor;

            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.QualityProfileId));
            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.MetadataProfileId));

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(authorPathValidator)
                           .SetValidator(authorAncestorValidator)
                           .SetValidator(systemFolderValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(qualityProfileExistsValidator);
            SharedValidator.RuleFor(s => s.MetadataProfileId).SetValidator(metadataProfileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.AuthorName).NotEmpty();
            PostValidator.RuleFor(s => s.ForeignAuthorId).NotEmpty().SetValidator(authorExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private AuthorResource GetAuthor(int id)
        {
            var author = _authorService.GetAuthor(id);
            return GetAuthorResource(author);
        }

        private AuthorResource GetAuthorResource(NzbDrone.Core.Books.Author author)
        {
            if (author == null)
            {
                return null;
            }

            var resource = author.ToResource();
            MapCoversToLocal(resource);
            FetchAndLinkAuthorStatistics(resource);
            LinkNextPreviousBooks(resource);

            //PopulateAlternateTitles(resource);
            LinkRootFolderPath(resource);

            return resource;
        }

        private List<AuthorResource> AllAuthors()
        {
            var authorStats = _authorStatisticsService.AuthorStatistics();
            var authorResources = _authorService.GetAllAuthors().ToResource();

            MapCoversToLocal(authorResources.ToArray());
            LinkNextPreviousBooks(authorResources.ToArray());
            LinkAuthorStatistics(authorResources, authorStats);

            //PopulateAlternateTitles(seriesResources);
            return authorResources;
        }

        private int AddAuthor(AuthorResource authorResource)
        {
            var author = _addAuthorService.AddAuthor(authorResource.ToModel());

            return author.Id;
        }

        private void UpdateAuthor(AuthorResource authorResource)
        {
            var moveFiles = Request.GetBooleanQueryParameter("moveFiles");
            var author = _authorService.GetAuthor(authorResource.Id);

            if (moveFiles)
            {
                var sourcePath = author.Path;
                var destinationPath = authorResource.Path;

                _commandQueueManager.Push(new MoveAuthorCommand
                {
                    AuthorId = author.Id,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Trigger = CommandTrigger.Manual
                });
            }

            var model = authorResource.ToModel(author);

            _authorService.UpdateAuthor(model);

            BroadcastResourceChange(ModelAction.Updated, authorResource);
        }

        private void DeleteAuthor(int id)
        {
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");
            var addImportListExclusion = Request.GetBooleanQueryParameter("addImportListExclusion");

            _authorService.DeleteAuthor(id, deleteFiles, addImportListExclusion);
        }

        private void MapCoversToLocal(params AuthorResource[] authors)
        {
            foreach (var authorResource in authors)
            {
                _coverMapper.ConvertToLocalUrls(authorResource.Id, MediaCoverEntity.Author, authorResource.Images);
            }
        }

        private void LinkNextPreviousBooks(params AuthorResource[] authors)
        {
            var nextBooks = _bookService.GetNextBooksByAuthorMetadataId(authors.Select(x => x.AuthorMetadataId));
            var lastBooks = _bookService.GetLastBooksByAuthorMetadataId(authors.Select(x => x.AuthorMetadataId));

            foreach (var authorResource in authors)
            {
                authorResource.NextBook = nextBooks.FirstOrDefault(x => x.AuthorMetadataId == authorResource.AuthorMetadataId);
                authorResource.LastBook = lastBooks.FirstOrDefault(x => x.AuthorMetadataId == authorResource.AuthorMetadataId);
            }
        }

        private void FetchAndLinkAuthorStatistics(AuthorResource resource)
        {
            LinkAuthorStatistics(resource, _authorStatisticsService.AuthorStatistics(resource.Id));
        }

        private void LinkAuthorStatistics(List<AuthorResource> resources, List<AuthorStatistics> authorStatistics)
        {
            foreach (var author in resources)
            {
                var stats = authorStatistics.SingleOrDefault(ss => ss.AuthorId == author.Id);
                if (stats == null)
                {
                    continue;
                }

                LinkAuthorStatistics(author, stats);
            }
        }

        private void LinkAuthorStatistics(AuthorResource resource, AuthorStatistics authorStatistics)
        {
            resource.Statistics = authorStatistics.ToResource();
        }

        //private void PopulateAlternateTitles(List<AuthorResource> resources)
        //{
        //    foreach (var resource in resources)
        //    {
        //        PopulateAlternateTitles(resource);
        //    }
        //}

        //private void PopulateAlternateTitles(AuthorResource resource)
        //{
        //    var mappings = _sceneMappingService.FindByTvdbId(resource.TvdbId);

        //    if (mappings == null) return;

        //    resource.AlternateTitles = mappings.Select(v => new AlternateTitleResource { Title = v.Title, SeasonNumber = v.SeasonNumber, SceneSeasonNumber = v.SceneSeasonNumber }).ToList();
        //}
        private void LinkRootFolderPath(AuthorResource resource)
        {
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
        }

        public void Handle(BookImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetAuthorResource(message.Author));
        }

        public void Handle(BookEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetAuthorResource(message.Book.Author.Value));
        }

        public void Handle(BookFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, GetAuthorResource(message.BookFile.Author.Value));
        }

        public void Handle(AuthorUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetAuthorResource(message.Author));
        }

        public void Handle(AuthorEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetAuthorResource(message.Author));
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
            BroadcastResourceChange(ModelAction.Updated, GetAuthorResource(message.Author));
        }
    }
}
