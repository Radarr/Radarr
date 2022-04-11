using System.Collections.Generic;
using System.Linq;
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
                var tmdbIdString = FindGuid(item.Guids, "tmdb");
                var imdbId = FindGuid(item.Guids, "imdb");

                int.TryParse(tmdbIdString, out int tmdbId);

                movies.AddIfNotNull(new ImportListMovie()
                {
                    ImdbId = imdbId,
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

        private string FindGuid(List<PlexSectionItemGuid> guids, string prefix)
        {
            var scheme = $"{prefix}://";

            return guids?.FirstOrDefault((guid) => guid.Id.StartsWith(scheme))?.Id.Replace(scheme, "");
        }
    }
}
