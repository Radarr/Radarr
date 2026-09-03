using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.MediaCover;

namespace NzbDrone.Core.MetadataSource.TMDb
{
    public class TmdbImagesProxy : ITmdbImagesProxy
    {
        private const string TmdbImageBaseUrl = "https://image.tmdb.org/t/p/original";

        private readonly IHttpClient _httpClient;
        private readonly IRadarrCloudRequestBuilder _requestBuilder;
        private readonly Logger _logger;

        public TmdbImagesProxy(IHttpClient httpClient, IRadarrCloudRequestBuilder requestBuilder, Logger logger)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder;
            _logger = logger;
        }

        public MediaCover.MediaCover GetMovieLogo(int tmdbId)
        {
            try
            {
                var request = _requestBuilder.TMDB.Create()
                    .SetSegment("api", "3")
                    .SetSegment("route", "movie")
                    .SetSegment("id", tmdbId.ToString())
                    .SetSegment("secondaryRoute", "/images")
                    .AddQueryParam("include_image_language", "en,null")
                    .Accept(HttpAccept.Json)
                    .Build();

                request.AllowAutoRedirect = true;
                request.SuppressHttpError = true;

                var response = _httpClient.Get<TmdbMovieImagesResource>(request);

                if (response.HasHttpError)
                {
                    return null;
                }

                if (response.Resource == null || response.Resource.Logos == null || response.Resource.Logos.Length == 0)
                {
                    return null;
                }

                var logo = response.Resource.Logos
                    .OrderByDescending(l => l.Iso6391 == "en" || l.Iso6391 == null ? 1 : 0)
                    .ThenByDescending(l => l.VoteAverage)
                    .ThenByDescending(l => l.VoteCount)
                    .FirstOrDefault();

                if (logo == null || logo.FilePath.IsNullOrWhiteSpace())
                {
                    return null;
                }

                var filePath = logo.FilePath;
                if (filePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = string.Concat(filePath.AsSpan(0, filePath.Length - 4), ".png");
                }

                var url = TmdbImageBaseUrl + filePath;
                return new MediaCover.MediaCover(MediaCoverTypes.Clearlogo, url);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
