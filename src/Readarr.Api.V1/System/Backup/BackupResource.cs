using System;
using NzbDrone.Core.Backup;
using Readarr.Http.REST;

namespace Readarr.Api.V1.System.Backup
{
    public class BackupResource : RestResource
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public BackupType Type { get; set; }
        public DateTime Time { get; set; }
    }
}
