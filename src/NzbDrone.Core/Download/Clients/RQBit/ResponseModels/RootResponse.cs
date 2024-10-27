using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.rQbit;

public class RootResponse
{
    public Dictionary<string, string> apis { get; set; }
    public string server { get; set; }
    public string version { get; set; }
}
