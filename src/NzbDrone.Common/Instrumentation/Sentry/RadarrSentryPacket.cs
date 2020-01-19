using Newtonsoft.Json;
using SharpRaven.Data;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    public class RadarrSentryPacket : JsonPacket
    {
        private readonly JsonSerializerSettings _setting;

        public RadarrSentryPacket(string project, SentryEvent @event) :
            base(project, @event)
        {
            _setting = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        public override string ToString(Formatting formatting)
        {
            return JsonConvert.SerializeObject(this, formatting, _setting);
        }

        public override string ToString()
        {
            return ToString(Formatting.None);
        }
    }
}
