using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using Nancy;
using NzbDrone.Api;
using NzbDrone.Api.Movie;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.RadarrAPI;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Api.Movie
{
    public class AlternativeYearModule : NzbDroneRestModule<AlternativeYearResource>
    {
        private readonly IMovieService _movieService;
        private readonly IRadarrAPIClient _radarrApi;
        private readonly ICached<int> _yearCache;
        private readonly IEventAggregator _eventAggregator;

        public AlternativeYearModule(IMovieService movieService, IRadarrAPIClient radarrApi, ICacheManager cacheManager, IEventAggregator eventAggregator)
            : base("/altyear")
        {
            _movieService = movieService;
            _radarrApi = radarrApi;
            CreateResource = AddYear;
            GetResourceById = GetYear;
            _yearCache = cacheManager.GetCache<int>(GetType(), "altYears");
            _eventAggregator = eventAggregator;
        }

        private int AddYear(AlternativeYearResource altYear)
        {
            var id = new Random().Next();
            _yearCache.Set(id.ToString(), altYear.Year, TimeSpan.FromMinutes(1));
            var movie = _movieService.GetMovie(altYear.MovieId);
            var newYear = _radarrApi.AddNewAlternativeYear(altYear.Year, movie.TmdbId);
            movie.SecondaryYear = newYear.Year;
            movie.SecondaryYearSourceId = newYear.SourceId;
            _movieService.UpdateMovie(movie);
            _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));
            return id;
        }

        private AlternativeYearResource GetYear(int id)
        {
            return new AlternativeYearResource
            {
                Year = _yearCache.Find(id.ToString())
            };
        }
    }
}