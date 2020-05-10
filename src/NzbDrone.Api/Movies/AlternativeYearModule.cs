using NzbDrone.Common.Cache;
using Radarr.Http;

namespace NzbDrone.Api.Movies
{
    public class AlternativeYearModule : RadarrRestModule<AlternativeYearResource>
    {
        private readonly ICached<int> _yearCache;

        public AlternativeYearModule(ICacheManager cacheManager)
            : base("/altyear")
        {
            GetResourceById = GetYear;
            _yearCache = cacheManager.GetCache<int>(GetType(), "altYears");
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
