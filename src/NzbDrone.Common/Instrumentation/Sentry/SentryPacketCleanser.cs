using Newtonsoft.Json.Linq;

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
        }
    }
}
