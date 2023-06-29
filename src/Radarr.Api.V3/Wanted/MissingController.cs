using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.Extensions;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
    {
    [V3ApiController("wanted/missing")]
    public class MissingController : RestControllerWithSignalR<MovieResource, Movie>
        {
        private readonly IMovieService _movieService;
        private readonly IConfigService _configService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IMovieTranslationService _movieTranslationService;

        public MissingController(IBroadcastSignalRMessage signalRBroadcaster,
                        IMovieService movieService,
                        IUpgradableSpecification qualityUpgradableSpecification,
                        ICustomFormatCalculationService formatCalculator,
                        IConfigService configService,
                        IMovieTranslationService movieTranslationService)
       : base(signalRBroadcaster)
        {
            _movieService = movieService;
            _configService = configService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _movieTranslationService = movieTranslationService;
        }

        [NonAction]
        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        private MovieTranslation GetMovieTranslation(List<MovieTranslation> translations, MovieMetadata movie)
            {
            if ((Language)_configService.MovieInfoLanguage == Language.Original)
                {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            return translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage && t.MovieMetadataId == movie.Id);
        }

        protected MovieResource MapToResource(Movie movie)
            {
            if (movie == null)
               {
                return null;
            }

            var availDelay = _configService.AvailabilityDelay;
            var translations = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.Id);
            var translation = GetMovieTranslation(translations, movie.MovieMetadata);
            var resource = movie.ToResource(availDelay, translation, _qualityUpgradableSpecification);

            return resource;
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<MovieResource> GetMissingMovies(bool includeImages = false)
            {
            var pagingResource = Request.ReadPagingResourceFromRequest<MovieResource>();
            var pagingSpec = new PagingSpec<Movie>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            var monitoredFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "monitored");

            if (monitoredFilter != null && monitoredFilter.Value == "false")
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Monitored == true);
            }

            var resource = pagingSpec.ApplyToPage(_movieService.MoviesWithoutFiles, MapToResource);

            return resource;
        }
    }
}
