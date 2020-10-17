using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using Radarr.Http;
using Radarr.Http.Extensions;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Blacklist
{
    public class BlacklistModule : RadarrRestModule<BlacklistResource>
    {
        private readonly IBlacklistService _blacklistService;
        private readonly ICustomFormatCalculationService _formatCalculator;

        public BlacklistModule(IBlacklistService blacklistService,
                               ICustomFormatCalculationService formatCalculator)
        {
            _blacklistService = blacklistService;
            _formatCalculator = formatCalculator;

            GetResourcePaged = GetBlacklist;
            DeleteResource = DeleteBlacklist;

            Get("/movie", x => GetMovieBlacklist());
            Delete("/bulk", x => Remove());
        }

        private PagingResource<BlacklistResource> GetBlacklist(PagingResource<BlacklistResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<BlacklistResource, NzbDrone.Core.Blacklisting.Blacklist>("date", SortDirection.Descending);

            return ApplyToPage(_blacklistService.Paged, pagingSpec, (blacklist) => BlacklistResourceMapper.MapToResource(blacklist, _formatCalculator));
        }

        private List<BlacklistResource> GetMovieBlacklist()
        {
            var queryMovieId = Request.Query.MovieId;

            if (!queryMovieId.HasValue)
            {
                throw new BadRequestException("movieId is missing");
            }

            int movieId = Convert.ToInt32(queryMovieId.Value);

            return _blacklistService.GetByMovieId(movieId).Select(h => BlacklistResourceMapper.MapToResource(h, _formatCalculator)).ToList();
        }

        private void DeleteBlacklist(int id)
        {
            _blacklistService.Delete(id);
        }

        private object Remove()
        {
            var resource = Request.Body.FromJson<BlacklistBulkResource>();

            _blacklistService.Delete(resource.Ids);

            return new object();
        }
    }
}
