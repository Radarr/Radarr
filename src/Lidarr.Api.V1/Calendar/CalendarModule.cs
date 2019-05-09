using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Music;
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;
using Lidarr.Api.V1.Albums;
using Lidarr.Http.Extensions;
using NzbDrone.Core.MediaCover;

namespace Lidarr.Api.V1.Calendar
{
    public class CalendarModule : AlbumModuleWithSignalR
    {
        public CalendarModule(IAlbumService albumService,
                              IArtistStatisticsService artistStatisticsService,
                              IMapCoversToLocal coverMapper,
                              IUpgradableSpecification upgradableSpecification,
                              IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster, "calendar")
        {
            GetResourceAll = GetCalendar;
        }

        private List<AlbumResource> GetCalendar()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(2);
            var includeUnmonitored = Request.GetBooleanQueryParameter("unmonitored");
            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");

            //TODO: Add Album Image support to AlbumModuleWithSignalR
            var includeAlbumImages = Request.GetBooleanQueryParameter("includeAlbumImages");

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            var resources = MapToResource(_albumService.AlbumsBetweenDates(start, end, includeUnmonitored), includeArtist);

            return resources.OrderBy(e => e.ReleaseDate).ToList();
        }
    }
}
