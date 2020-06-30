using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Http
{
    public class CachedHttpResponse : ModelBase
    {
        public string Url { get; set; }
        public DateTime LastRefresh { get; set; }
        public DateTime Expiry { get; set; }
        public string Value { get; set; }
        public int StatusCode { get; set; }
    }
}
