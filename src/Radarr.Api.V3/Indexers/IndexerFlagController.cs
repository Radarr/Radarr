using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;
using Radarr.Http;

namespace Radarr.Api.V3.Indexers
{
    [V3ApiController]
    public class IndexerFlagController : Controller
    {
        [HttpGet]
        public List<IndexerFlagResource> GetAll()
        {
            var type = typeof(IndexerFlags);

            return Enum.GetValues(type)
                .Cast<IndexerFlags>()
                .Where(f => type.GetField(f.ToString())?.GetCustomAttributes(false).OfType<ObsoleteAttribute>().Empty() ?? true)
                .Select(f => new IndexerFlagResource
                {
                    Id = (int)f,
                    Name = f.ToString()
                }).ToList();
        }
    }
}
