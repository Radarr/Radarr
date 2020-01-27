using Radarr.Http.REST;

namespace Radarr.Api.V3.DiskSpace
{
    public class DiskSpaceResource : RestResource
    {
        public string Path { get; set; }
        public string Label { get; set; }
        public long FreeSpace { get; set; }
        public long TotalSpace { get; set; }
    }

    public static class DiskSpaceResourceMapper
    {
        public static DiskSpaceResource MapToResource(this NzbDrone.Core.DiskSpace.DiskSpace model)
        {
            if (model == null)
            {
                return null;
            }

            return new DiskSpaceResource
            {
                Path = model.Path,
                Label = model.Label,
                FreeSpace = model.FreeSpace,
                TotalSpace = model.TotalSpace
            };
        }
    }
}
