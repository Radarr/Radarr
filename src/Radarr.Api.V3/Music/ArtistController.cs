using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Music
{
    [V3ApiController]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S107", Justification = "DI pattern")]
    public class ArtistController : RestControllerWithSignalR<ArtistResource, Artist>,
                                    IHandle<ArtistAddedEvent>,
                                    IHandle<ArtistEditedEvent>,
                                    IHandle<ArtistDeletedEvent>
    {
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addArtistService;
        private readonly IRootFolderService _rootFolderService;

        public ArtistController(IBroadcastSignalRMessage signalRBroadcaster,
                                IArtistService artistService,
                                IAddArtistService addArtistService,
                                IRootFolderService rootFolderService,
                                RootFolderValidator rootFolderValidator,
                                MappedNetworkDriveValidator mappedNetworkDriveValidator,
                                RecycleBinValidator recycleBinValidator,
                                SystemFolderValidator systemFolderValidator,
                                QualityProfileExistsValidator qualityProfileExistsValidator,
                                RootFolderExistsValidator rootFolderExistsValidator)
            : base(signalRBroadcaster)
        {
            _artistService = artistService;
            _addArtistService = addArtistService;
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
        public List<ArtistResource> GetArtists()
        {
            var artists = _artistService.GetAllArtists();
            var resources = artists.ToResource();
            var rootFolders = _rootFolderService.All();

            foreach (var resource in resources)
            {
                resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path, rootFolders);
            }

            return resources;
        }

        protected override ArtistResource GetResourceById(int id)
        {
            var artist = _artistService.GetArtist(id);
            return MapToResource(artist);
        }

        private ArtistResource MapToResource(Artist artist)
        {
            if (artist == null)
            {
                return null;
            }

            var resource = artist.ToResource();
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);

            return resource;
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<ArtistResource> AddArtist([FromBody] ArtistResource artistResource)
        {
            var artist = _addArtistService.AddArtist(artistResource.ToModel());
            return Created(artist.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<ArtistResource> UpdateArtist([FromBody] ArtistResource artistResource)
        {
            var artist = _artistService.GetArtist(artistResource.Id);
            var updatedArtist = _artistService.UpdateArtist(artistResource.ToModel(artist));
            var resource = MapToResource(updatedArtist);

            BroadcastResourceChange(ModelAction.Updated, resource);

            return Ok(resource);
        }

        [RestDeleteById]
        public ActionResult DeleteArtist(int id, bool deleteFiles = false)
        {
            _artistService.DeleteArtist(id, deleteFiles);
            return NoContent();
        }

        [NonAction]
        public void Handle(ArtistAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Artist));
        }

        [NonAction]
        public void Handle(ArtistEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Artist));
        }

        [NonAction]
        public void Handle(ArtistDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Artist.Id);
        }
    }
}
