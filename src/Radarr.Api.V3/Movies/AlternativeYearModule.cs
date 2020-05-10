using NzbDrone.Common.Cache;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    public class AlternativeYearModule : RadarrRestModule<AlternativeYearResource>
    {
        private readonly IMovieService _movieService;
        private readonly ICached<int> _yearCache;
        private readonly IEventAggregator _eventAggregator;

        public AlternativeYearModule(IMovieService movieService, ICacheManager cacheManager, IEventAggregator eventAggregator)
            : base("/altyear")
        {
            _movieService = movieService;
            GetResourceById = GetYear;
            _yearCache = cacheManager.GetCache<int>(GetType(), "altYears");
            _eventAggregator = eventAggregator;
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
