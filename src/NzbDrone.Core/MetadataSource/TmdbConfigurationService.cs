using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.MediaCover;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Cloud;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.MetadataSource
{
    public interface ITmdbConfigService
    {
        MediaCover.MediaCover GetCoverForURL(string url, MediaCover.MediaCoverTypes type);
    }

    class TmdbConfigService : ITmdbConfigService
    {
        private readonly ICached<ConfigResource> _configurationCache;
        private readonly IHttpClient _httpClient;
        private readonly IHttpRequestBuilderFactory _tmdbBuilder;

        public TmdbConfigService(ICacheManager cacheManager, IHttpClient httpClient, ISonarrCloudRequestBuilder requestBuilder)
        {
            _configurationCache = cacheManager.GetCache<ConfigResource>(GetType(), "configuration_cache");
            _httpClient = httpClient;
            _tmdbBuilder = requestBuilder.TMDBSingle;
        }

        public MediaCover.MediaCover GetCoverForURL(string url, MediaCover.MediaCoverTypes type)
        {
            if (_configurationCache.Count == 0)
            {
                RefreshCache();
            }

            var images = _configurationCache.Find("configuration").images;

            var cover = new MediaCover.MediaCover();
            cover.CoverType = type;

            var realUrl = images.base_url;

            switch (type)
            {
                case MediaCoverTypes.Fanart:
                    realUrl += images.backdrop_sizes.Last();
                    break;
                case MediaCoverTypes.Poster:
                    realUrl += images.poster_sizes.Last();
                    break;
                default:
                    realUrl += "original";
                    break;
            }

            realUrl += url;

            cover.Url = realUrl;

            return cover;
        }

        private void RefreshCache()
        {
            var request = _tmdbBuilder.Create().SetSegment("route", "configuration").Build();

            var response = _httpClient.Get<ConfigResource>(request);

            if (response.Resource.images != null)
            {
                _configurationCache.Set("configuration", response.Resource);
            }
        }
    }
}
