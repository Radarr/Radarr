using NzbDrone.Core.Datastore.Events;

namespace NzbDrone.SignalR
{
    public class SignalRMessage
    {
        public object Body { get; set; }
        public string Name { get; set; }

#if !NETCOREAPP
        [Newtonsoft.Json.JsonIgnore]
#else
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public ModelAction Action { get; set; }
    }
}
