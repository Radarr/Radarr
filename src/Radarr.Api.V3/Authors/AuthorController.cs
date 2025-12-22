using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authors;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Authors
{
    [V3ApiController]
    public class AuthorController : RestControllerWithSignalR<AuthorResource, Author>
    {
        private readonly IAuthorService _authorService;
        private readonly IRootFolderService _rootFolderService;

        public AuthorController(IBroadcastSignalRMessage signalRBroadcaster,
                                IAuthorService authorService,
                                IRootFolderService rootFolderService,
                                RootFolderValidator rootFolderValidator,
                                MappedNetworkDriveValidator mappedNetworkDriveValidator,
                                RecycleBinValidator recycleBinValidator,
                                SystemFolderValidator systemFolderValidator,
                                QualityProfileExistsValidator qualityProfileExistsValidator,
                                RootFolderExistsValidator rootFolderExistsValidator)
            : base(signalRBroadcaster)
        {
            _authorService = authorService;
            _rootFolderService = rootFolderService;

            SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(recycleBinValidator)
                .SetValidator(systemFolderValidator)
                .When(s => s.Path.IsNotNullOrWhiteSpace());

            PostValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .When(s => s.Path.IsNullOrWhiteSpace());

            PutValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath();

            SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.Name).NotEmpty();
        }

        [HttpGet]
        public List<AuthorResource> GetAuthors()
        {
            var authors = _authorService.GetAllAuthors();
            var resources = authors.ToResource();
            var rootFolders = _rootFolderService.All();

            foreach (var resource in resources)
            {
                resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path, rootFolders);
            }

            return resources;
        }

        protected override AuthorResource GetResourceById(int id)
        {
            var author = _authorService.GetAuthor(id);
            return MapToResource(author);
        }

        private AuthorResource MapToResource(Author author)
        {
            if (author == null)
            {
                return null;
            }

            var resource = author.ToResource();
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);

            return resource;
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<AuthorResource> AddAuthor([FromBody] AuthorResource authorResource)
        {
            var author = _authorService.AddAuthor(authorResource.ToModel());
            return Created(author.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<AuthorResource> UpdateAuthor([FromBody] AuthorResource authorResource)
        {
            var author = _authorService.GetAuthor(authorResource.Id);
            var updatedAuthor = _authorService.UpdateAuthor(authorResource.ToModel(author));
            var resource = MapToResource(updatedAuthor);

            BroadcastResourceChange(ModelAction.Updated, resource);

            return Ok(resource);
        }

        [RestDeleteById]
        public ActionResult DeleteAuthor(int id, bool deleteFiles = false)
        {
            _authorService.DeleteAuthor(id, deleteFiles);
            return NoContent();
        }
    }
}
