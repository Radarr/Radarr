using System;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource.RadarrAPI;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using Radarr.Http;

namespace NzbDrone.Api.Movies
{
    public class AlternativeYearModule : RadarrRestModule<AlternativeYearResource>
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
