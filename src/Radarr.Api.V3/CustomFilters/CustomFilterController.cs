using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.CustomFilters;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.CustomFilters
{
    [V3ApiController]
    public class CustomFilterController : RestController<CustomFilterResource>
    {
        private readonly ICustomFilterService _customFilterService;

        public CustomFilterController(ICustomFilterService customFilterService)
        {
            _customFilterService = customFilterService;
        }

        protected override CustomFilterResource GetResourceById(int id)
        {
            return _customFilterService.Get(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<CustomFilterResource>> GetCustomFilters()
        {
            return TypedResults.Ok(_customFilterService.All().ToResource());
        }

        [RestPostById]
        [Consumes("application/json")]
        public Results<Created<CustomFilterResource>, NotFound> AddCustomFilter([FromBody] CustomFilterResource resource)
        {
            var customFilter = _customFilterService.Add(resource.ToModel());

            return Created(customFilter.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public Results<Accepted<CustomFilterResource>, NotFound> UpdateCustomFilter([FromBody] CustomFilterResource resource)
        {
            _customFilterService.Update(resource.ToModel());
            return Accepted(resource.Id);
        }

        [RestDeleteById]
        public Ok DeleteCustomResource(int id)
        {
            _customFilterService.Delete(id);

            return TypedResults.Ok();
        }
    }
}
