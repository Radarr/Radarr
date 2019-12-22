using System;
using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Indexers
{
    public class IndexerStatus : ProviderStatusBase
    {
        public ReleaseInfo LastRssSyncReleaseInfo { get; set; }

        public IDictionary<string, string> Cookies { get; set; }
        public DateTime? CookiesExpirationDate { get; set; }
    }
}
