using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.ImportLists.ImportExclusions;
using Radarr.Http;
using Radarr.Http.Extensions;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.ImportLists
{
    [V3ApiController("exclusions")]
    public class ImportListExclusionController : RestController<ImportListExclusionResource>
    {
        private readonly IImportListExclusionService _importListExclusionService;

        public ImportListExclusionController(IImportListExclusionService importListExclusionService,
                                             ImportListExclusionExistsValidator importListExclusionExistsValidator)
        {
            _importListExclusionService = importListExclusionService;

            SharedValidator.RuleFor(c => c.TmdbId).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(importListExclusionExistsValidator);

            SharedValidator.RuleFor(c => c.MovieTitle).NotEmpty();
            SharedValidator.RuleFor(c => c.MovieYear).GreaterThan(0);
        }

        [HttpGet]
        [Produces("application/json")]
        [Obsolete("Deprecated")]
        public List<ImportListExclusionResource> GetImportListExclusions()
        {
            return _importListExclusionService.All().ToResource();
        }

        protected override ImportListExclusionResource GetResourceById(int id)
        {
            return _importListExclusionService.Get(id).ToResource();
        }

        [HttpGet("paged")]
        [Produces("application/json")]
        public PagingResource<ImportListExclusionResource> GetImportListExclusionsPaged([FromQuery] PagingRequestResource paging)
        {
            var pagingResource = new PagingResource<ImportListExclusionResource>(paging);
            var pageSpec = pagingResource.MapToPagingSpec<ImportListExclusionResource, ImportListExclusion>(
                "id",
                SortDirection.Descending,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "id",
                    "tmdbId",
                    "movieTitle",
                    "movieYear"
                });

            return pageSpec.ApplyToPage(_importListExclusionService.Paged, ImportListExclusionResourceMapper.ToResource);
        }

        [RestPostById]
        [Consumes("application/json")]
        public ActionResult<ImportListExclusionResource> AddImportListExclusion([FromBody] ImportListExclusionResource resource)
        {
            var importListExclusion = _importListExclusionService.Add(resource.ToModel());

            return Created(importListExclusion.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<ImportListExclusionResource> UpdateImportListExclusion([FromBody] ImportListExclusionResource resource)
        {
            _importListExclusionService.Update(resource.ToModel());
            return Accepted(resource.Id);
        }

        [HttpPost("bulk")]
        public object AddImportListExclusions([FromBody] List<ImportListExclusionResource> resources)
        {
            var importListExclusions = _importListExclusionService.Add(resources.ToModel());

            return importListExclusions.ToResource();
        }

        [RestDeleteById]
        public void DeleteImportListExclusion(int id)
        {
            _importListExclusionService.Delete(id);
        }

        [HttpDelete("bulk")]
        [Produces("application/json")]
        public object DeleteImportListExclusions([FromBody] ImportListExclusionBulkResource resource)
        {
            _importListExclusionService.Delete(resource.Ids.ToList());

            return new { };
        }
    }
}
