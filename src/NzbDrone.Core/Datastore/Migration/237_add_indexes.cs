using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(237)]
    public class add_indexes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index().OnTable("Blocklist").OnColumn("MovieId");
            Create.Index().OnTable("Blocklist").OnColumn("Date");

            Create.Index()
                .OnTable("History")
                .OnColumn("MovieId").Ascending()
                .OnColumn("Date").Descending();

            Delete.Index().OnTable("History").OnColumn("DownloadId");
            Create.Index()
                .OnTable("History")
                .OnColumn("DownloadId").Ascending()
                .OnColumn("Date").Descending();

            Create.Index().OnTable("Movies").OnColumn("MovieFileId");
            Create.Index().OnTable("Movies").OnColumn("Path");
        }
    }
}
