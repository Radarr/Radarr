using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.MediaCover;

namespace NzbDrone.Core.ImportLists.TMDb
{
    public class TMDbParser : IParseImportListResponse
    {
        public virtual IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            var movies = new List<ImportListMovie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<MovieSearchResource>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.Results.SelectList(MapListMovie);
        }

        protected ImportListMovie MapListMovie(MovieResultResource movieResult)
        {
            var movie =  new ImportListMovie
            {
                TmdbId = movieResult.Id,
                Title = movieResult.Title,
            };

            if (movieResult.ReleaseDate.IsNotNullOrWhiteSpace())
            {
                DateTime.TryParse(movieResult.ReleaseDate, out var releaseDate);
                movie.Year = releaseDate.Year;
            }

            return movie;
        }

        private MediaCover.MediaCover MapPosterImage(string path)
        {
            if (path.IsNotNullOrWhiteSpace())
            {
                return new MediaCover.MediaCover(MediaCoverTypes.Poster, $"https://image.tmdb.org/t/p/original{path}");
            }

            return null;
        }

        protected virtual bool PreProcess(ImportListResponse listResponse)
        {
            if (listResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(listResponse,
                    "TMDb API call resulted in an unexpected StatusCode [{0}]",
                    listResponse.HttpResponse.StatusCode);
            }

            if (listResponse.HttpResponse.Headers.ContentType != null &&
                listResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                listResponse.HttpRequest.Headers.Accept != null &&
                !listResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(listResponse,
                    "TMDb responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
