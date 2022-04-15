namespace NzbDrone.Core.DecisionEngine
{
    public class Decision
    {
        public bool Accepted { get; private set; }
        public string Reason { get; private set; }
        public int ProfileId { get; private set; }

        private static readonly Decision AcceptDecision = new Decision { Accepted = true };
        private Decision()
        {
        }

        public static Decision Accept()
        {
            return AcceptDecision;
        }

        public static Decision Reject(string reason, int profileId = 0)
        {
            return new Decision
            {
                Accepted = false,
                Reason = reason,
                ProfileId = profileId
            };
        }
    }
}
