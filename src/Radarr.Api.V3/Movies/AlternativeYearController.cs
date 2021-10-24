using NzbDrone.Common.Cache;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("altyear")]
    public class AlternativeYearController : RestController<AlternativeYearResource>
    {
        private readonly ICached<int> _yearCache;

        public AlternativeYearController(ICacheManager cacheManager)
        {
            _yearCache = cacheManager.GetCache<int>(GetType(), "altYears");
        }

        protected override AlternativeYearResource GetResourceById(int id)
        {
            return new AlternativeYearResource
            {
                Year = _yearCache.Find(id.ToString())
            };
        }
    }
}
