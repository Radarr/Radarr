using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Audiobooks;
using NzbDrone.Core.Audiobooks.Events;
using NzbDrone.Core.AudiobookStats;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Api.V3.MediaItems;
using Radarr.Http;

namespace Radarr.Api.V3.Audiobooks
{
    [V3ApiController]
    public class AudiobookController : BaseMediaCrudController<AudiobookResource, Audiobook>,
                                       IHandle<AudiobookAddedEvent>,
                                       IHandle<AudiobookEditedEvent>,
                                       IHandle<AudiobooksDeletedEvent>,
                                       IHandle<AudiobooksBulkEditedEvent>
    {
        private readonly IAudiobookService _audiobookService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IHierarchicalMonitoringService _monitoringService;
        private readonly IAudiobookStatisticsService _audiobookStatisticsService;

        protected override IBaseMediaService<Audiobook> MediaService => _audiobookService;
        protected override IRootFolderService RootFolderService => _rootFolderService;

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

            SetupPathValidation(rootFolderValidator, mappedNetworkDriveValidator, recycleBinValidator, systemFolderValidator, rootFolderExistsValidator);
            SetupQualityValidation(qualityProfileExistsValidator);
            SetupTitleValidation();
        }

        protected override string GetPath(AudiobookResource resource) => resource.Path;
        protected override string GetRootFolderPath(AudiobookResource resource) => resource.RootFolderPath;
        protected override int GetQualityProfileId(AudiobookResource resource) => resource.QualityProfileId;
        protected override string GetTitle(AudiobookResource resource) => resource.Title;

        protected override AudiobookResource MapToResource(Audiobook audiobook)
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

        protected override Audiobook ResourceToModel(AudiobookResource resource) => resource.ToModel();
        protected override Audiobook ApplyResourceToModel(AudiobookResource resource, Audiobook audiobook) => resource.ToModel(audiobook);

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
