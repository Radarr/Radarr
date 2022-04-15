namespace NzbDrone.Core.DecisionEngine
{
    public class Rejection
    {
        public string Reason { get; set; }
        public RejectionType Type { get; set; }

        public int ProfileId { get; set; }

        public Rejection(string reason, int profileId = 0, RejectionType type = RejectionType.Permanent)
        {
            Reason = reason;
            ProfileId = profileId;
            Type = type;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Type, Reason);
        }
    }
}
