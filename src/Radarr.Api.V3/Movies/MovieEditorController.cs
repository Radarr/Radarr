using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Translations;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("movie/editor")]
    public class MovieEditorController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IMovieTranslationService _movieTranslationService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IConfigService _configService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IUpgradableSpecification _upgradableSpecification;

        public MovieEditorController(IMovieService movieService,
            IMovieTranslationService movieTranslationService,
            IMapCoversToLocal coverMapper,
            IConfigService configService,
            IManageCommandQueue commandQueueManager,
            IUpgradableSpecification upgradableSpecification)
        {
            _movieService = movieService;
            _movieTranslationService = movieTranslationService;
            _coverMapper = coverMapper;
            _configService = configService;
            _commandQueueManager = commandQueueManager;
            _upgradableSpecification = upgradableSpecification;
        }

        [HttpPut]
        public IActionResult SaveAll([FromBody] MovieEditorResource resource)
        {
            var moviesToUpdate = _movieService.GetMovies(resource.MovieIds);
            var moviesToMove = new List<BulkMoveMovie>();

            foreach (var movie in moviesToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    movie.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    movie.QualityProfileId = resource.QualityProfileId.Value;
                }

                if (resource.MinimumAvailability.HasValue)
                {
                    movie.MinimumAvailability = resource.MinimumAvailability.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    movie.RootFolderPath = resource.RootFolderPath;
                    moviesToMove.Add(new BulkMoveMovie
                    {
                        MovieId = movie.Id,
                        SourcePath = movie.Path
                    });
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => movie.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => movie.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            movie.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }
            }

            if (resource.MoveFiles && moviesToMove.Any())
            {
                _commandQueueManager.Push(new BulkMoveMovieCommand
                {
                    DestinationRootFolder = resource.RootFolderPath,
                    Movies = moviesToMove
                });
            }

            var configLanguage = (Language)_configService.MovieInfoLanguage;
            var availabilityDelay = _configService.AvailabilityDelay;

            var translations = _movieTranslationService.GetAllTranslationsForLanguage(configLanguage);
            var tdict = translations.ToDictionary(x => x.MovieMetadataId);

            var updatedMovies = _movieService.UpdateMovie(moviesToUpdate, !resource.MoveFiles);

            var moviesResources = new List<MovieResource>(updatedMovies.Count);

            foreach (var movie in updatedMovies)
            {
                var translation = GetTranslationFromDict(tdict, movie.MovieMetadata, configLanguage);
                var movieResource = movie.ToResource(availabilityDelay, translation, _upgradableSpecification);

                MapCoversToLocal(movieResource);

                moviesResources.Add(movieResource);
            }

            return Accepted(moviesResources);
        }

        [HttpDelete]
        public object DeleteMovies([FromBody] MovieEditorResource resource)
        {
            _movieService.DeleteMovies(resource.MovieIds, resource.DeleteFiles, resource.AddImportExclusion);

            return new { };
        }

        private MovieTranslation GetTranslationFromDict(Dictionary<int, MovieTranslation> translations, MovieMetadata movie, Language configLanguage)
        {
            if (configLanguage == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            if (!translations.TryGetValue(movie.Id, out var translation))
            {
                translation = new MovieTranslation
                {
                    Title = movie.Title,
                    Language = Language.English
                };
            }

            return translation;
        }

        private void MapCoversToLocal(MovieResource movie)
        {
            _coverMapper.ConvertToLocalUrls(movie.Id, movie.Images);
        }
    }
}
