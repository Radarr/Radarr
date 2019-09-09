using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Lidarr.Http.Extensions;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.Tracks
{
    public class TrackModule : TrackModuleWithSignalR
    {
        public TrackModule(IArtistService artistService,
                             ITrackService trackService,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(trackService, artistService, upgradableSpecification, signalRBroadcaster)
        {
            GetResourceAll = GetTracks;
        }

        private List<TrackResource> GetTracks()
        {
            var artistIdQuery = Request.Query.ArtistId;
            var albumIdQuery = Request.Query.AlbumId;
            var albumReleaseIdQuery = Request.Query.AlbumReleaseId;
            var trackIdsQuery = Request.Query.TrackIds;

            if (!artistIdQuery.HasValue && !trackIdsQuery.HasValue && !albumIdQuery.HasValue && !albumReleaseIdQuery.HasValue)
            {
                throw new BadRequestException("One of artistId, albumId, albumReleaseId or trackIds must be provided");
            }

            if (artistIdQuery.HasValue && !albumIdQuery.HasValue)
            {
                int artistId = Convert.ToInt32(artistIdQuery.Value);

                return MapToResource(_trackService.GetTracksByArtist(artistId), false, false);
            }

            if (albumReleaseIdQuery.HasValue)
            {
                int releaseId = Convert.ToInt32(albumReleaseIdQuery.Value);

                return MapToResource(_trackService.GetTracksByRelease(releaseId), false, false);
            }

            if (albumIdQuery.HasValue)
            {
                int albumId = Convert.ToInt32(albumIdQuery.Value);

                return MapToResource(_trackService.GetTracksByAlbum(albumId), false, false);
            }

            string trackIdsValue = trackIdsQuery.Value.ToString();

            var trackIds = trackIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => Convert.ToInt32(e))
                                            .ToList();

            return MapToResource(_trackService.GetTracks(trackIds), false, false);
        }
    }
}
