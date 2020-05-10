using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.Exceptions;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbParser : IParseNetImportResponse
    {
        private readonly ISearchForNewMovie _skyhookProxy;

        public TMDbParser(ISearchForNewMovie skyhookProxy)
        {
            _skyhookProxy = skyhookProxy;
        }

        public virtual IList<Movie> ParseResponse(NetImportResponse importResponse)
        {
            var movies = new List<Movie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<MovieSearchRoot>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.Results.SelectList(m => new Movie { TmdbId = m.id });
        }

        protected virtual bool PreProcess(NetImportResponse listResponse)
        {
            if (listResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(listResponse,
                    "TMDb API call resulted in an unexpected StatusCode [{0}]",
                    listResponse.HttpResponse.StatusCode);
            }

            if (listResponse.HttpResponse.Headers.ContentType != null &&
                listResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                listResponse.HttpRequest.Headers.Accept != null &&
                !listResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new NetImportException(listResponse,
                    "TMDb responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
