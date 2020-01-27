using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tags;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Tags
{
    public class TagDetailsResource : RestResource
    {
        public string Label { get; set; }
        public List<int> DelayProfileIds { get; set; }
        public List<int> NotificationIds { get; set; }
        public List<int> RestrictionIds { get; set; }
        public List<int> NetImportIds { get; set; }
        public List<int> MovieIds { get; set; }
    }

    public static class TagDetailsResourceMapper
    {
        public static TagDetailsResource ToResource(this TagDetails model)
        {
            if (model == null)
            {
                return null;
            }

            return new TagDetailsResource
            {
                Id = model.Id,
                Label = model.Label,
                DelayProfileIds = model.DelayProfileIds,
                NotificationIds = model.NotificationIds,
                RestrictionIds = model.RestrictionIds,
                NetImportIds = model.NetImportIds,
                MovieIds = model.MovieIds
            };
        }

        public static List<TagDetailsResource> ToResource(this IEnumerable<TagDetails> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
