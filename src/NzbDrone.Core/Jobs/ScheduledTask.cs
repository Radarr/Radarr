using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Jobs
{
    public class ScheduledTask : ModelBase
    {
        public string TypeName { get; set; }
        public double Interval { get; set; }
        public DateTime LastExecution { get; set; }
    }
}