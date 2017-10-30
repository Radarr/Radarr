using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Restrictions;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Tags
{
    public class TagDetails : ModelBase
    {
        public string Label { get; set; }
        public List<Artist> Artist { get; set; }
        public List<NotificationDefinition> Notifications { get; set; }
        public List<Restriction> Restrictions { get; set; }
        public List<DelayProfile> DelayProfiles { get; set; }
    }
}
