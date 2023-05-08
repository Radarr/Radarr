using System.Collections.Generic;

namespace Radarr.Http
{
    public class ApiInfoResource
    {
        public string Current { get; set; }
        public List<string> Deprecated { get; set; }
    }
}
