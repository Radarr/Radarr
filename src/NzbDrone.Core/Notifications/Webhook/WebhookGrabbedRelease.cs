using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookGrabbedRelease
    {
        public WebhookGrabbedRelease()
        {
        }

        public WebhookGrabbedRelease(GrabbedReleaseInfo release)
        {
            if (release == null)
            {
                return;
            }

            ReleaseTitle = release.Title;
            Indexer = release.Indexer;
            Size = release.Size;
            IndexerFlags = GetListOfIndexerFlags(release.IndexerFlags);
        }

        public WebhookGrabbedRelease(GrabbedReleaseInfo release, IndexerFlags indexerFlags)
        {
            if (release == null)
            {
                IndexerFlags = GetListOfIndexerFlags(indexerFlags);

                return;
            }

            ReleaseTitle = release.Title;
            Indexer = release.Indexer;
            Size = release.Size;
            IndexerFlags = GetListOfIndexerFlags(release.IndexerFlags);
        }

        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
        public List<string> IndexerFlags { get; set; }

        private static List<string> GetListOfIndexerFlags(IndexerFlags indexerFlags)
        {
            return Enum.GetValues(typeof(IndexerFlags))
                .Cast<IndexerFlags>()
                .Where(f => (indexerFlags & f) == f)
                .Select(f => f.ToString())
                .ToList();
        }
    }
}
