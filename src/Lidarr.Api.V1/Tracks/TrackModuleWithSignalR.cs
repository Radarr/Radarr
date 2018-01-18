using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.SignalR;
using Lidarr.Api.V1.TrackFiles;
using Lidarr.Api.V1.Artist;
using Lidarr.Http;
using NzbDrone.Core.MediaFiles.Events;

namespace Lidarr.Api.V1.Tracks
{
    public abstract class TrackModuleWithSignalR : LidarrRestModuleWithSignalR<TrackResource, Track>,
            IHandle<TrackInfoRefreshedEvent>,
            IHandle<TrackImportedEvent>
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

            GetResourceById = GetTrack;
        }

        protected TrackModuleWithSignalR(ITrackService trackService,
                                           IArtistService artistService,
                                           IUpgradableSpecification upgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _trackService = trackService;
            _artistService = artistService;
            _upgradableSpecification = upgradableSpecification;

            GetResourceById = GetTrack;
        }

        protected TrackResource GetTrack(int id)
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

        protected List<TrackResource> MapToResource(List<Track> tracks, bool includeArtist, bool includeTrackFile)
        {
            var result = tracks.ToResource();

            if (includeArtist || includeTrackFile)
            {
                var artistDict = new Dictionary<int, NzbDrone.Core.Music.Artist>();
                for (var i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    var resource = result[i];

                    var series = track.Artist ?? artistDict.GetValueOrDefault(tracks[i].ArtistId) ?? _artistService.GetArtist(tracks[i].ArtistId);
                    artistDict[series.Id] = series;

                    if (includeArtist)
                    {
                        resource.Artist = series.ToResource();
                    }
                    if (includeTrackFile && tracks[i].TrackFileId != 0)
                    {
                        resource.TrackFile = tracks[i].TrackFile.Value.ToResource(series, _upgradableSpecification);
                    }
                }
            }

            return result;
        }

        public void Handle(TrackInfoRefreshedEvent message)
        {
            foreach (var track in message.Removed)
            {
                BroadcastResourceChange(ModelAction.Deleted, track.ToResource());
            }

            foreach (var track in message.Added)
            {
                BroadcastResourceChange(ModelAction.Updated, track.ToResource());
            }

            foreach (var track in message.Updated)
            {
                BroadcastResourceChange(ModelAction.Updated, track.Id);
            }
        }

        public void Handle(TrackImportedEvent message)
        {
            foreach (var track in message.TrackInfo.Tracks)
            {
                BroadcastResourceChange(ModelAction.Updated, track.Id);
            }
        }

    }
}
