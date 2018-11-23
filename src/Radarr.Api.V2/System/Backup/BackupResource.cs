using System;
using Radarr.Http.REST;
using NzbDrone.Core.Backup;

namespace Radarr.Api.V2.System.Backup
{
    public class BackupResource : RestResource
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public BackupType Type { get; set; }
        public DateTime Time { get; set; }
    }
}
