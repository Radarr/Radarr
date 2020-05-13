using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecision
    {
        public RemoteBook RemoteBook { get; private set; }
        public IEnumerable<Rejection> Rejections { get; private set; }

        public bool Approved => !Rejections.Any();

        public bool TemporarilyRejected
        {
            get
            {
                return Rejections.Any() && Rejections.All(r => r.Type == RejectionType.Temporary);
            }
        }

        public bool Rejected
        {
            get
            {
                return Rejections.Any() && Rejections.Any(r => r.Type == RejectionType.Permanent);
            }
        }

        public DownloadDecision(RemoteBook book, params Rejection[] rejections)
        {
            RemoteBook = book;
            Rejections = rejections.ToList();
        }

        public override string ToString()
        {
            if (Approved)
            {
                return "[OK] " + RemoteBook;
            }

            return "[Rejected " + Rejections.Count() + "]" + RemoteBook;
        }
    }
}
