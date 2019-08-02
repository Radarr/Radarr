using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using Lidarr.Api.V1.Albums;
using Lidarr.Api.V1.Artist;
using Lidarr.Api.V1.Tracks;
using Lidarr.Http;
using Lidarr.Http.Extensions;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.History
{
    public class HistoryModule : LidarrRestModule<HistoryResource>
    {
        private readonly IHistoryService _historyService;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryModule(IHistoryService historyService,
                             IUpgradableSpecification upgradableSpecification,
                             IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            _upgradableSpecification = upgradableSpecification;
            _failedDownloadService = failedDownloadService;
            GetResourcePaged = GetHistory;

            Get["/since"] = x => GetHistorySince();
            Get["/artist"] = x => GetArtistHistory();
            Post["/failed"] = x => MarkAsFailed();
        }

        protected HistoryResource MapToResource(NzbDrone.Core.History.History model, bool includeArtist, bool includeAlbum, bool includeTrack)
        {
            var resource = model.ToResource();

            if (includeArtist)
            {
                resource.Artist = model.Artist.ToResource();
            }
            if (includeAlbum)
            {
                resource.Album = model.Album.ToResource();
            }
            if (includeTrack)
            {
                resource.Track = model.Track.ToResource();
            }
            

            if (model.Artist != null)
            {
                resource.QualityCutoffNotMet = _upgradableSpecification.QualityCutoffNotMet(model.Artist.QualityProfile.Value, model.Quality);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, NzbDrone.Core.History.History>("date", SortDirection.Descending);
            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");
            var includeAlbum = Request.GetBooleanQueryParameter("includeAlbum");
            var includeTrack = Request.GetBooleanQueryParameter("includeTrack");

            var eventTypeFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "eventType");
            var albumIdFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "albumId");

            if (eventTypeFilter != null)
            {
                var filterValue = (HistoryEventType)Convert.ToInt32(eventTypeFilter.Value);
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (albumIdFilter != null)
            {
                var albumId = Convert.ToInt32(albumIdFilter.Value);
                pagingSpec.FilterExpressions.Add(h => h.AlbumId == albumId);
            }


            return ApplyToPage(_historyService.Paged, pagingSpec, h => MapToResource(h, includeArtist, includeAlbum, includeTrack));
        }

        private List<HistoryResource> GetHistorySince()
        {
            var queryDate = Request.Query.Date;
            var queryEventType = Request.Query.EventType;

            if (!queryDate.HasValue)
            {
                throw new BadRequestException("date is missing");
            }

            DateTime date = DateTime.Parse(queryDate.Value);
            HistoryEventType? eventType = null;
            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");
            var includeAlbum = Request.GetBooleanQueryParameter("includeAlbum");
            var includeTrack = Request.GetBooleanQueryParameter("includeTrack");

            if (queryEventType.HasValue)
            {
                eventType = (HistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeArtist, includeAlbum, includeTrack)).ToList();
        }

        private List<HistoryResource> GetArtistHistory()
        {
            var queryArtistId = Request.Query.ArtistId;
            var queryAlbumId = Request.Query.AlbumId;
            var queryEventType = Request.Query.EventType;

            if (!queryArtistId.HasValue)
            {
                throw new BadRequestException("artistId is missing");
            }

            int artistId = Convert.ToInt32(queryArtistId.Value);
            HistoryEventType? eventType = null;
            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");
            var includeAlbum = Request.GetBooleanQueryParameter("includeAlbum");
            var includeTrack = Request.GetBooleanQueryParameter("includeTrack");

            if (queryEventType.HasValue)
            {
                eventType = (HistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            if (queryAlbumId.HasValue)
            {
                int albumId = Convert.ToInt32(queryAlbumId.Value);

                return _historyService.GetByAlbum(albumId, eventType).Select(h => MapToResource(h, includeArtist, includeAlbum, includeTrack)).ToList();
            }

            return _historyService.GetByArtist(artistId, eventType).Select(h => MapToResource(h, includeArtist, includeAlbum, includeTrack)).ToList();
        }

        private Response MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _failedDownloadService.MarkAsFailed(id);
            return new object().AsResponse();
        }
    }
}
