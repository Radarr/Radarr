using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Indexers;
using Radarr.Http;

namespace Radarr.Api.V3.Specifications
{
    [V3ApiController]
    public class SpecificationsController : Controller
    {
        private readonly IIndexerFactory _indexerFactory;

        public SpecificationsController(IIndexerFactory indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }

        [HttpPost("action/indexers")]
        public object GetIndexers()
        {
            return new
            {
                options = _indexerFactory.All().Select(o => new FieldSelectOption { Value = o.Id, Name = o.Name }).ToList()
            };
        }
    }
}
