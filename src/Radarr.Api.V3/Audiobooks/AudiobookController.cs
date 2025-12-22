using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Audiobooks;
using NzbDrone.Core.Audiobooks.Events;
using NzbDrone.Core.AudiobookStats;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Audiobooks
{
    [V3ApiController]
    public class AudiobookController : RestControllerWithSignalR<AudiobookResource, Audiobook>,
                                       IHandle<AudiobookAddedEvent>,
                                       IHandle<AudiobookEditedEvent>,
                                       IHandle<AudiobooksDeletedEvent>,
                                       IHandle<AudiobooksBulkEditedEvent>
    {
        private readonly IAudiobookService _audiobookService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IHierarchicalMonitoringService _monitoringService;
        private readonly IAudiobookStatisticsService _audiobookStatisticsService;

        public AudiobookController(IBroadcastSignalRMessage signalRBroadcaster,
                                   IAudiobookService audiobookService,
                                   IRootFolderService rootFolderService,
                                   IHierarchicalMonitoringService monitoringService,
                                   IAudiobookStatisticsService audiobookStatisticsService,
                                   RootFolderValidator rootFolderValidator,
                                   MappedNetworkDriveValidator mappedNetworkDriveValidator,
                                   RecycleBinValidator recycleBinValidator,
                                   SystemFolderValidator systemFolderValidator,
                                   QualityProfileExistsValidator qualityProfileExistsValidator,
                                   RootFolderExistsValidator rootFolderExistsValidator)
            : base(signalRBroadcaster)
        {
            _audiobookService = audiobookService;
            _rootFolderService = rootFolderService;
            _monitoringService = monitoringService;
            _audiobookStatisticsService = audiobookStatisticsService;

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

            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        [HttpGet]
        public List<AudiobookResource> GetAudiobooks(int? authorId = null, int? seriesId = null, int? bookId = null, string narrator = null)
        {
            List<Audiobook> audiobooks;

            if (authorId.HasValue)
            {
                audiobooks = _audiobookService.FindByAuthorId(authorId.Value);
            }
            else if (seriesId.HasValue)
            {
                audiobooks = _audiobookService.FindBySeriesId(seriesId.Value);
            }
            else if (bookId.HasValue)
            {
                audiobooks = _audiobookService.FindByBookId(bookId.Value);
            }
            else if (narrator.IsNotNullOrWhiteSpace())
            {
                audiobooks = _audiobookService.FindByNarrator(narrator);
            }
            else
            {
                audiobooks = _audiobookService.GetAllAudiobooks();
            }

            var resources = audiobooks.ToResource();
            var rootFolders = _rootFolderService.All();
            var audiobookStats = _audiobookStatisticsService.AudiobookStatistics();
            var sdict = audiobookStats.ToDictionary(x => x.AudiobookId);

            for (var i = 0; i < resources.Count; i++)
            {
                resources[i].RootFolderPath = _rootFolderService.GetBestRootFolderPath(resources[i].Path, rootFolders);
                resources[i].EffectivelyMonitored = _monitoringService.IsEffectivelyMonitored(audiobooks[i]);
            }

            LinkAudiobookStatistics(resources, sdict);

            return resources;
        }

        protected override AudiobookResource GetResourceById(int id)
        {
            var audiobook = _audiobookService.GetAudiobook(id);
            return MapToResource(audiobook);
        }

        private AudiobookResource MapToResource(Audiobook audiobook)
        {
            if (audiobook == null)
            {
                return null;
            }

            var resource = audiobook.ToResource();
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
            resource.EffectivelyMonitored = _monitoringService.IsEffectivelyMonitored(audiobook);
            FetchAndLinkAudiobookStatistics(resource);

            return resource;
        }

        private void FetchAndLinkAudiobookStatistics(AudiobookResource resource)
        {
            LinkAudiobookStatistics(resource, _audiobookStatisticsService.AudiobookStatistics(resource.Id));
        }

        private void LinkAudiobookStatistics(List<AudiobookResource> resources, Dictionary<int, AudiobookStatistics> sDict)
        {
            foreach (var audiobook in resources)
            {
                if (sDict.TryGetValue(audiobook.Id, out var stats))
                {
                    LinkAudiobookStatistics(audiobook, stats);
                }
            }
        }

        private static void LinkAudiobookStatistics(AudiobookResource resource, AudiobookStatistics audiobookStatistics)
        {
            resource.Statistics = audiobookStatistics.ToResource();
            resource.HasFile = audiobookStatistics.AudiobookFileCount > 0;
            resource.SizeOnDisk = audiobookStatistics.SizeOnDisk;
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<AudiobookResource> AddAudiobook([FromBody] AudiobookResource audiobookResource)
        {
            var audiobook = _audiobookService.AddAudiobook(audiobookResource.ToModel());
            return Created(audiobook.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<AudiobookResource> UpdateAudiobook([FromBody] AudiobookResource audiobookResource)
        {
            var audiobook = _audiobookService.GetAudiobook(audiobookResource.Id);
            var updatedAudiobook = _audiobookService.UpdateAudiobook(audiobookResource.ToModel(audiobook));
            var resource = MapToResource(updatedAudiobook);

            BroadcastResourceChange(ModelAction.Updated, resource);

            return Ok(resource);
        }

        [RestDeleteById]
        public ActionResult DeleteAudiobook(int id, bool deleteFiles = false)
        {
            _audiobookService.DeleteAudiobook(id, deleteFiles);
            return NoContent();
        }

        [NonAction]
        public void Handle(AudiobookAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Audiobook));
        }

        [NonAction]
        public void Handle(AudiobookEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Audiobook));
        }

        [NonAction]
        public void Handle(AudiobooksDeletedEvent message)
        {
            foreach (var audiobook in message.Audiobooks)
            {
                BroadcastResourceChange(ModelAction.Deleted, audiobook.Id);
            }
        }

        [NonAction]
        public void Handle(AudiobooksBulkEditedEvent message)
        {
            foreach (var audiobook in message.Audiobooks)
            {
                BroadcastResourceChange(ModelAction.Updated, MapToResource(audiobook));
            }
        }
    }
}
