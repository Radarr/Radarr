using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;
using NzbDrone.Core.RootFolders;
using Readarr.Http.REST;

namespace Readarr.Api.V1.RootFolders
{
    public class RootFolderResource : RestResource
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int DefaultMetadataProfileId { get; set; }
        public int DefaultQualityProfileId { get; set; }
        public MonitorTypes DefaultMonitorOption { get; set; }
        public HashSet<int> DefaultTags { get; set; }

        public bool Accessible { get; set; }
        public long? FreeSpace { get; set; }
        public long? TotalSpace { get; set; }
    }

    public static class RootFolderResourceMapper
    {
        public static RootFolderResource ToResource(this RootFolder model)
        {
            if (model == null)
            {
                return null;
            }

            return new RootFolderResource
            {
                Id = model.Id,

                Name = model.Name,
                Path = model.Path,
                DefaultMetadataProfileId = model.DefaultMetadataProfileId,
                DefaultQualityProfileId = model.DefaultQualityProfileId,
                DefaultMonitorOption = model.DefaultMonitorOption,
                DefaultTags = model.DefaultTags,

                Accessible = model.Accessible,
                FreeSpace = model.FreeSpace,
                TotalSpace = model.TotalSpace,
            };
        }

        public static RootFolder ToModel(this RootFolderResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new RootFolder
            {
                Id = resource.Id,
                Name = resource.Name,
                Path = resource.Path,

                DefaultMetadataProfileId = resource.DefaultMetadataProfileId,
                DefaultQualityProfileId = resource.DefaultQualityProfileId,
                DefaultMonitorOption = resource.DefaultMonitorOption,
                DefaultTags = resource.DefaultTags
            };
        }

        public static List<RootFolderResource> ToResource(this IEnumerable<RootFolder> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
