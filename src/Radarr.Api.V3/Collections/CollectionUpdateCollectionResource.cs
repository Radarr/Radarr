using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radarr.Api.V3.Collections
{
    public class CollectionUpdateCollectionResource
    {
        public int Id { get; set; }
        public bool? Monitored { get; set; }
    }
}
