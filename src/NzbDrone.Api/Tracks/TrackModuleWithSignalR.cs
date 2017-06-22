using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Api.TrackFiles;
using NzbDrone.Api.Music;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Tracks
{
    public abstract class TrackModuleWithSignalR : NzbDroneRestModuleWithSignalR<TrackResource, Track>,
        IHandle<TrackDownloadedEvent>
    {
        protected readonly ITrackService _trackService;
        protected readonly IArtistService _artistService;
        protected readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;

        protected TrackModuleWithSignalR(ITrackService trackService,
                                           IArtistService artistService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _trackService = trackService;
            _artistService = artistService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetTrack;
        }

        protected TrackModuleWithSignalR(ITrackService trackService,
                                           IArtistService artistService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _trackService = trackService;
            _artistService = artistService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetTrack;
        }

        protected TrackResource GetTrack(int id)
        {
            var track = _trackService.GetTrack(id);
            var resource = MapToResource(track, true, true);
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
                    resource.TrackFile = track.TrackFile.Value.ToResource(artist, _qualityUpgradableSpecification);
                }
            }

            return resource;
        }

        protected List<TrackResource> MapToResource(List<Track> tracks, bool includeArtist, bool includeTrackFile)
        {
            var result = tracks.ToResource();

            if (includeArtist || includeTrackFile)
            {
                var artistDict = new Dictionary<int, Core.Music.Artist>();
                for (var i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    var resource = result[i];

                    var artist = track.Artist ?? artistDict.GetValueOrDefault(tracks[i].ArtistId) ?? _artistService.GetArtist(tracks[i].ArtistId);
                    artistDict[artist.Id] = artist;
                    
                    if (includeArtist)
                    {
                        resource.Artist = artist.ToResource();
                    }
                    if (includeTrackFile && tracks[i].TrackFileId != 0)
                    {
                        resource.TrackFile = tracks[i].TrackFile.Value.ToResource(artist, _qualityUpgradableSpecification);
                    }
                }
            }

            return result;
        }

        //public void Handle(TrackGrabbedEvent message)
        //{
        //    foreach (var track in message.Track.Tracks)
        //    {
        //        var resource = track.ToResource();
        //        resource.Grabbed = true;

        //        BroadcastResourceChange(ModelAction.Updated, resource);
        //    }
        //}

        public void Handle(TrackDownloadedEvent message)
        {
            foreach (var track in message.Track.Tracks)
            {
                BroadcastResourceChange(ModelAction.Updated, track.Id);
            }
        }
    }
}
