using System.Collections.Generic;
using NzbDrone.Core.DiskSpace;
using Radarr.Http;

namespace NzbDrone.Api.DiskSpace
{
    public class DiskSpaceModule :RadarrRestModule<DiskSpaceResource>
    {
        private readonly IDiskSpaceService _diskSpaceService;

        public DiskSpaceModule(IDiskSpaceService diskSpaceService)
            : base("diskspace")
        {
            _diskSpaceService = diskSpaceService;
            GetResourceAll = GetFreeSpace;
        }


        public List<DiskSpaceResource> GetFreeSpace()
        {
            return _diskSpaceService.GetFreeSpace().ConvertAll(DiskSpaceResourceMapper.MapToResource);
        }
    }
}
