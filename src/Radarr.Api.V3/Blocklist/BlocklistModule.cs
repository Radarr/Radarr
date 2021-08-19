using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using Radarr.Http;
using Radarr.Http.Extensions;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Blocklist
{
    public class BlocklistModule : RadarrRestModule<BlocklistResource>
    {
        private readonly IBlocklistService _blocklistService;
        private readonly ICustomFormatCalculationService _formatCalculator;

        public BlocklistModule(IBlocklistService blocklistService,
                               ICustomFormatCalculationService formatCalculator)
        {
            _blocklistService = blocklistService;
            _formatCalculator = formatCalculator;

            GetResourcePaged = GetBlocklist;
            DeleteResource = DeleteBlocklist;

            Get("/movie", x => GetMovieBlocklist());
            Delete("/bulk", x => Remove());
        }

        private PagingResource<BlocklistResource> GetBlocklist(PagingResource<BlocklistResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, NzbDrone.Core.Blocklisting.Blocklist>("date", SortDirection.Descending);

            return ApplyToPage(_blocklistService.Paged, pagingSpec, (blocklist) => BlocklistResourceMapper.MapToResource(blocklist, _formatCalculator));
        }

        private List<BlocklistResource> GetMovieBlocklist()
        {
            var queryMovieId = Request.Query.MovieId;

            if (!queryMovieId.HasValue)
            {
                throw new BadRequestException("movieId is missing");
            }

            int movieId = Convert.ToInt32(queryMovieId.Value);

            return _blocklistService.GetByMovieId(movieId).Select(h => BlocklistResourceMapper.MapToResource(h, _formatCalculator)).ToList();
        }

        private void DeleteBlocklist(int id)
        {
            _blocklistService.Delete(id);
        }

        private object Remove()
        {
            var resource = Request.Body.FromJson<BlocklistBulkResource>();

            _blocklistService.Delete(resource.Ids);

            return new object();
        }
    }
}
