using Newtonsoft.Json.Linq;
using SharpRaven.Data;
using System;
using System.Linq;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    public class SentryPacketCleanser
    {
        public void CleansePacket(RadarrSentryPacket packet)
        {
            packet.Message = CleanseLogMessage.Cleanse(packet.Message);

            if (packet.Fingerprint != null)
            {
                for (var i = 0; i < packet.Fingerprint.Length; i++)
                {
                    packet.Fingerprint[i] = CleanseLogMessage.Cleanse(packet.Fingerprint[i]);
                }
            }

            if (packet.Extra != null)
            {
                var target = JObject.FromObject(packet.Extra);
                new CleansingJsonVisitor().Visit(target);
                packet.Extra = target;
            }

            if (packet.Breadcrumbs != null)
            {
                for (var i = 0; i < packet.Breadcrumbs.Count; i++)
                {
                    packet.Breadcrumbs[i] = CleanseBreadcrumb(packet.Breadcrumbs[i]);
                }
            }
        }

        private static Breadcrumb CleanseBreadcrumb(Breadcrumb b)
        {
            try
            {
                var message = CleanseLogMessage.Cleanse(b.Message);
                var data = b.Data?.ToDictionary(x => x.Key, y => CleanseLogMessage.Cleanse(y.Value));
                return new Breadcrumb(b.Category) { Message = message, Type = b.Type, Data = data, Level = b.Level };
            }
            catch (Exception)
            {
            }

            return b;
        }
    }
}
