using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Notifications.Plex.Server;

namespace NzbDrone.Core.ImportLists.Plex
{
    public class PlexParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;

        public PlexParser()
        {
        }

        public virtual IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            List<PlexSectionItem> items;

            _importResponse = importResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            items = Json.Deserialize<PlexResponse<PlexSectionResponse>>(_importResponse.Content)
                        .MediaContainer
                        .Items;

            foreach (var item in items)
            {
                var tmdbIdString = item.Guids?.Find((guid) => guid.Id.StartsWith("tmdb://"))?.Id.Replace("tmdb://", "");
                var tmdbId = 0;
                if (tmdbIdString.IsNotNullOrWhiteSpace())
                {
                    tmdbId = int.Parse(tmdbIdString);
                }

                movies.AddIfNotNull(new ImportListMovie()
                {
                    ImdbId = item.Guids?.Find((guid) => guid.Id.StartsWith("imdb://"))?.Id.Replace("imdb://", ""),
                    TmdbId = tmdbId,
                    Title = item.Title,
                    Year = item.Year
                });
            }

            return movies;
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "Plex API call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            if (importListResponse.HttpResponse.Headers.ContentType != null && importListResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                importListResponse.HttpRequest.Headers.Accept != null && !importListResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(importListResponse, "Plex API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
