using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Lidarr.Api.V3.TrackFiles;
using Lidarr.Api.V3.Artist;
using Lidarr.Http;

namespace Lidarr.Api.V3.Tracks
{
    public abstract class TrackModuleWithSignalR : LidarrRestModuleWithSignalR<TrackResource, Track>
        //IHandle<EpisodeGrabbedEvent>,
        //IHandle<EpisodeImportedEvent>
    {
        protected readonly ITrackService _trackService;
        protected readonly IArtistService _artistService;
        protected readonly IUpgradableSpecification _upgradableSpecification;

        protected TrackModuleWithSignalR(ITrackService trackService,
                                           IArtistService artistService,
                                           IUpgradableSpecification upgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _trackService = trackService;
            _artistService = artistService;
            _upgradableSpecification = upgradableSpecification;

            GetResourceById = GetEpisode;
        }

        protected TrackModuleWithSignalR(ITrackService episodeService,
                                           IArtistService seriesService,
                                           IUpgradableSpecification upgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _trackService = episodeService;
            _artistService = seriesService;
            _upgradableSpecification = upgradableSpecification;

            GetResourceById = GetEpisode;
        }

        protected TrackResource GetEpisode(int id)
        {
            var episode = _trackService.GetTrack(id);
            var resource = MapToResource(episode, true, true);
            return resource;
        }

        protected TrackResource MapToResource(Track track, bool includeArtist, bool includeTrackFile)
        {
            var resource = track.ToResource();

            if (includeArtist || includeTrackFile)
            {
                var artist = track.Artist ?? _artistService.GetArtist(track.ArtistId);

                if (includeArtist)
                {
                    resource.Artist = artist.ToResource();
                }
                if (includeTrackFile && track.TrackFileId != 0)
                {
                    resource.TrackFile = track.TrackFile.Value.ToResource(artist, _upgradableSpecification);
                }
            }

            return resource;
        }

        protected List<TrackResource> MapToResource(List<Track> episodes, bool includeSeries, bool includeEpisodeFile)
        {
            var result = episodes.ToResource();

            if (includeSeries || includeEpisodeFile)
            {
                var seriesDict = new Dictionary<int, NzbDrone.Core.Music.Artist>();
                for (var i = 0; i < episodes.Count; i++)
                {
                    var episode = episodes[i];
                    var resource = result[i];

                    var series = episode.Artist ?? seriesDict.GetValueOrDefault(episodes[i].ArtistId) ?? _artistService.GetArtist(episodes[i].ArtistId);
                    seriesDict[series.Id] = series;

                    if (includeSeries)
                    {
                        resource.Artist = series.ToResource();
                    }
                    if (includeEpisodeFile && episodes[i].TrackFileId != 0)
                    {
                        resource.TrackFile = episodes[i].TrackFile.Value.ToResource(series, _upgradableSpecification);
                    }
                }
            }

            return result;
        }

        //public void Handle(EpisodeGrabbedEvent message)
        //{
        //    foreach (var episode in message.Episode.Episodes)
        //    {
        //        var resource = episode.ToResource();
        //        resource.Grabbed = true;

        //        BroadcastResourceChange(ModelAction.Updated, resource);
        //    }
        //}

        //public void Handle(EpisodeImportedEvent message)
        //{
        //    if (!message.NewDownload)
        //    {
        //        return;
        //    }

        //    foreach (var episode in message.EpisodeInfo.Episodes)
        //    {
        //        BroadcastResourceChange(ModelAction.Updated, episode.Id);
        //    }
        //}
    }
}
