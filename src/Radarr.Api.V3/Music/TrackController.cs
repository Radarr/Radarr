using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring;
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
    public class TrackController : RestControllerWithSignalR<TrackResource, Track>,
                                   IHandle<TrackAddedEvent>,
                                   IHandle<TracksDeletedEvent>
    {
        private readonly ITrackService _trackService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IHierarchicalMonitoringService _monitoringService;

        public TrackController(IBroadcastSignalRMessage signalRBroadcaster,
                               ITrackService trackService,
                               IRootFolderService rootFolderService,
                               IHierarchicalMonitoringService monitoringService,
                               RootFolderValidator rootFolderValidator,
                               MappedNetworkDriveValidator mappedNetworkDriveValidator,
                               RecycleBinValidator recycleBinValidator,
                               SystemFolderValidator systemFolderValidator,
                               QualityProfileExistsValidator qualityProfileExistsValidator)
            : base(signalRBroadcaster)
        {
            _trackService = trackService;
            _rootFolderService = rootFolderService;
            _monitoringService = monitoringService;

            SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(recycleBinValidator)
                .SetValidator(systemFolderValidator)
                .When(s => s.Path.IsNotNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        [HttpGet]
        public List<TrackResource> GetTracks(int? albumId = null)
        {
            List<Track> tracks;

            if (albumId.HasValue)
            {
                tracks = _trackService.FindByAlbumId(albumId.Value);
            }
            else
            {
                tracks = _trackService.GetAllTracks();
            }

            var resources = tracks.ToResource();
            var rootFolders = _rootFolderService.All();

            for (var i = 0; i < resources.Count; i++)
            {
                if (resources[i].Path.IsNotNullOrWhiteSpace())
                {
                    resources[i].RootFolderPath = _rootFolderService.GetBestRootFolderPath(resources[i].Path, rootFolders);
                }

                resources[i].EffectivelyMonitored = _monitoringService.IsEffectivelyMonitored(tracks[i]);
            }

            return resources;
        }

        protected override TrackResource GetResourceById(int id)
        {
            var track = _trackService.GetTrack(id);
            return MapToResource(track);
        }

        private TrackResource MapToResource(Track track)
        {
            if (track == null)
            {
                return null;
            }

            var resource = track.ToResource();

            if (resource.Path.IsNotNullOrWhiteSpace())
            {
                resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
            }

            resource.EffectivelyMonitored = _monitoringService.IsEffectivelyMonitored(track);

            return resource;
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<TrackResource> AddTrack([FromBody] TrackResource trackResource)
        {
            var track = _trackService.AddTrack(trackResource.ToModel());
            return Created(track.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<TrackResource> UpdateTrack([FromBody] TrackResource trackResource)
        {
            var track = _trackService.GetTrack(trackResource.Id);
            var updatedTrack = _trackService.UpdateTrack(trackResource.ToModel(track));
            var resource = MapToResource(updatedTrack);

            BroadcastResourceChange(ModelAction.Updated, resource);

            return Ok(resource);
        }

        [RestDeleteById]
        public ActionResult DeleteTrack(int id, bool deleteFiles = false)
        {
            _trackService.DeleteTrack(id, deleteFiles);
            return NoContent();
        }

        [NonAction]
        public void Handle(TrackAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Track));
        }

        [NonAction]
        public void Handle(TracksDeletedEvent message)
        {
            foreach (var track in message.Tracks)
            {
                BroadcastResourceChange(ModelAction.Deleted, track.Id);
            }
        }
    }
}
