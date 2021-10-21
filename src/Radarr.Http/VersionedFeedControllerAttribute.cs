using System;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Radarr.Http
{
    public class VersionedFeedControllerAttribute : Attribute, IRouteTemplateProvider
    {
        public VersionedFeedControllerAttribute(int version, string resource = "[controller]")
        {
            Version = version;
            Template = $"feed/v{Version}/{resource}";
        }

        public string Template { get; private set; }
        public int? Order => 2;
        public string Name { get; set; }
        public int Version { get; private set; }
    }

    public class V3FeedControllerAttribute : VersionedApiControllerAttribute
    {
        public V3FeedControllerAttribute(string resource = "[controller]")
            : base(3, resource)
        {
        }
    }
}
