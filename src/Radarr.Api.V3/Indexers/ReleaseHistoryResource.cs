using System;

namespace Radarr.Api.V3.Indexers;

public class ReleaseHistoryResource
{
    public DateTime? Grabbed { get; set; }
    public DateTime? Failed { get; set; }
}
