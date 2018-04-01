using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class IndexerStatus : ModelBase
    {
        public int IndexerId { get; set; }

        public DateTime? InitialFailure { get; set; }
        public DateTime? MostRecentFailure { get; set; }
        public int EscalationLevel { get; set; }
        public DateTime? DisabledTill { get; set; }

        public ReleaseInfo LastRssSyncReleaseInfo { get; set; }
        
        public IDictionary<string, string> Cookies { get; set; }
        public DateTime? CookiesExpirationDate { get; set; }

        public bool IsDisabled()
        {
            return DisabledTill.HasValue && DisabledTill.Value > DateTime.UtcNow;
        }
    }
}
