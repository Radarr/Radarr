using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Music;
using NzbDrone.Core.Tv; //TODO Remove after EpisodeCutoffService is Refactored 
using NzbDrone.Core.ArtistStats;
using NzbDrone.SignalR;
using Lidarr.Api.V3.Albums;
using Lidarr.Http;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V3.Wanted
{
    public class CutoffModule : AlbumModuleWithSignalR
    {
        private readonly IEpisodeCutoffService _episodeCutoffService;

        public CutoffModule(IEpisodeCutoffService episodeCutoffService,
                            IAlbumService albumService,
                            IArtistStatisticsService artistStatisticsService,
                            IArtistService artistService,
                            IUpgradableSpecification upgradableSpecification,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(albumService, artistStatisticsService, artistService, upgradableSpecification, signalRBroadcaster, "wanted/cutoff")
        {
            _episodeCutoffService = episodeCutoffService;
            GetResourcePaged = GetCutoffUnmetAlbums;
        }

        private PagingResource<AlbumResource> GetCutoffUnmetAlbums(PagingResource<AlbumResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Album>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");
            var includeTrackFile = Request.GetBooleanQueryParameter("includeTrackFile");

            if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "false")
            {
                pagingSpec.FilterExpression = v => v.Monitored == false || v.Artist.Monitored == false;
            }
            else
            {
                pagingSpec.FilterExpression = v => v.Monitored == true && v.Artist.Monitored == true;
            }

            //var resource = ApplyToPage(_episodeCutoffService.EpisodesWhereCutoffUnmet, pagingSpec, v => MapToResource(v, includeSeries, includeEpisodeFile));
            return null;
            //return resource;
        }
    }
}
