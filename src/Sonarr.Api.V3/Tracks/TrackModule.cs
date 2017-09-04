using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Lidarr.Http.Extensions;
using Lidarr.Http.REST;

namespace Lidarr.Api.V3.Tracks
{
    public class TrackModule : TrackModuleWithSignalR
    {
        public TrackModule(IArtistService artistService,
                             ITrackService trackService,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(trackService, artistService, upgradableSpecification, signalRBroadcaster)
        {
            GetResourceAll = GetEpisodes;
        }

        private List<TrackResource> GetEpisodes()
        {
            var artistIdQuery = Request.Query.ArtistId;
            var trackIdsQuery = Request.Query.TrackIds;

            if (!artistIdQuery.HasValue && !trackIdsQuery.HasValue)
            {
                throw new BadRequestException("artistId or trackIds must be provided");
            }

            if (artistIdQuery.HasValue)
            {
                int artistId = Convert.ToInt32(artistIdQuery.Value);
                var albumId = Request.Query.AlbumId.HasValue ? (int)Request.Query.AlbumId : (int?)null;

                if (albumId.HasValue)
                {
                    return MapToResource(_trackService.GetTracksByAlbum(artistId, albumId.Value), false, false);
                }

                return MapToResource(_trackService.GetTracksByArtist(artistId), false, false);
            }

            string trackIdsValue = trackIdsQuery.Value.ToString();

            var trackIds = trackIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => Convert.ToInt32(e))
                                            .ToList();

            return MapToResource(_trackService.GetTracks(trackIds), false, false);
        }
    }
}
