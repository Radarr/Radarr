using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;
using Radarr.Http;

namespace Radarr.Api.V3.Indexers
{
    public class IndexerFlagModule : RadarrRestModule<IndexerFlagResource>
    {
        public IndexerFlagModule()
        {
            GetResourceAll = GetAll;
        }

        private List<IndexerFlagResource> GetAll()
        {
            return Enum.GetValues(typeof(IndexerFlags)).Cast<IndexerFlags>().Select(f => new IndexerFlagResource
            {
                Id = (int)f,
                Name = f.ToString()
            }).ToList();
        }
    }
}
