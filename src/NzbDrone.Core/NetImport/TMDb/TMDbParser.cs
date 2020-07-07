using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.Exceptions;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbParser : IParseNetImportResponse
    {
        public virtual IList<Movie> ParseResponse(NetImportResponse importResponse)
        {
            var movies = new List<Movie>();

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

        protected Movie MapListMovie(MovieResultResource movieResult)
        {
            var movie =  new Movie
            {
                TmdbId = movieResult.Id,
                Overview = movieResult.Overview,
                Title = movieResult.OriginalTitle,
                SortTitle = Parser.Parser.NormalizeTitle(movieResult.OriginalTitle),
                Images = new List<MediaCover.MediaCover>()
            };

            if (movieResult.ReleaseDate.IsNotNullOrWhiteSpace())
            {
                DateTime.TryParse(movieResult.ReleaseDate, out var releaseDate);
                movie.Year = releaseDate.Year;
            }

            movie.Images.AddIfNotNull(MapPosterImage(movieResult.PosterPath));

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
