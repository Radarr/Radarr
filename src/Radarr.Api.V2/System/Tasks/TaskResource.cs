using System;
using Radarr.Http.REST;

namespace Radarr.Api.V2.System.Tasks
{
    public class TaskResource : RestResource
    {
        public string Name { get; set; }
        public string TaskName { get; set; }
        public int Interval { get; set; }
        public DateTime LastExecution { get; set; }
        public DateTime NextExecution { get; set; }
    }
}
