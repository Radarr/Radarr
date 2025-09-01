using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class RootResponse
{
    public Dictionary<string, string> Apis { get; set; }
    public string Server { get; set; }
    public string Version { get; set; }
}
