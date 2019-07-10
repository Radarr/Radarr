﻿using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.MetadataSource;
using TinyIoC;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbParser : IParseNetImportResponse
    {
        private readonly TMDbSettings _settings;
        private NetImportResponse _importResponse;
        private readonly ISearchForNewMovie _skyhookProxy;
        private readonly Logger _logger;

        public TMDbParser(TMDbSettings settings, ISearchForNewMovie skyhookProxy)
        {
            _skyhookProxy = skyhookProxy;
            _settings = settings;
        }

        public IList<Movies.Movie> ParseResponse(NetImportResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<Movies.Movie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            switch (_settings.ListType)
            {
                case (int)TMDbListType.List:
                    movies = ProcessListResponse();
                    break;
                case (int)TMDbListType.PeopleCast:
                    movies = ProcessPersonCastListResponse();
                    break;
                case (int)TMDbListType.PeopleCrew:
                    movies = ProcessPersonCrewListResponse();
                    break;
                default:
                    movies = ProcessDiscoveryListResponse();
                    break;
            }

            return movies;
        }

        protected List<Movies.Movie> ProcessPersonCrewListResponse()
        {
            var jsonResponse = JsonConvert.DeserializeObject<PersonCreditsRoot>(_importResponse.Content);
            var movies = new List<Movies.Movie>();

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            var creditsDepartment = _settings.GetCrewDepartments();

            foreach (var movie in jsonResponse.crew)
            {
                if (creditsDepartment.Contains(movie.department))
                {
                    movies.AddIfNotNull(_skyhookProxy.MapMovie(movie));
                }
            }

            return movies;
        }
        protected List<Movies.Movie> ProcessPersonCastListResponse()
        {
            var jsonResponse = JsonConvert.DeserializeObject<PersonCreditsRoot>(_importResponse.Content);
            var movies = new List<Movies.Movie>();

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.cast.SelectList(_skyhookProxy.MapMovie);
        }

        protected List<Movies.Movie> ProcessDiscoveryListResponse()
        {
            var jsonResponse = JsonConvert.DeserializeObject<MovieSearchRoot>(_importResponse.Content);
            var movies = new List<Movies.Movie>();

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.results.SelectList(_skyhookProxy.MapMovie);
        }

        protected List<Movies.Movie> ProcessListResponse()
        {
            var jsonResponse = JsonConvert.DeserializeObject<ListResponseRoot>(_importResponse.Content);
            var movies = new List<Movies.Movie>();

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            foreach (var movie in jsonResponse.items)
            {
                // Skip non-movie things
                if (movie.media_type != "movie")
                {
                    continue;
                }

                // Movies with no Year Fix
                if (string.IsNullOrWhiteSpace(movie.release_date))
                {
                    continue;
                }

                movies.AddIfNotNull(_skyhookProxy.MapMovie(movie));
            }

            return movies;
        }

        protected virtual bool PreProcess(NetImportResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(indexerResponse, "Indexer API call resulted in an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
            }

            if (indexerResponse.HttpResponse.Headers.ContentType != null && indexerResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                indexerResponse.HttpRequest.Headers.Accept != null && !indexerResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new NetImportException(indexerResponse, "Indexer responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

    }
}