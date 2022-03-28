using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(153)]
    public class indexer_client_status_search_changes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Cleanup cases of Sonarr Interference with Radarr db
            if (Schema.Table("PendingReleases").Column("Reason").Exists())
            {
                Delete.Column("Reason").FromTable("PendingReleases");
            }

            Alter.Table("PendingReleases").AddColumn("Reason").AsInt32().WithDefaultValue(0);

            Rename.Column("IndexerId").OnTable("IndexerStatus").To("ProviderId");

            Rename.Column("EnableSearch").OnTable("Indexers").To("EnableAutomaticSearch");
            Alter.Table("Indexers").AddColumn("EnableInteractiveSearch").AsBoolean().Nullable();

            Execute.Sql("UPDATE \"Indexers\" SET \"EnableInteractiveSearch\" = \"EnableAutomaticSearch\"");

            Alter.Table("Indexers").AlterColumn("EnableInteractiveSearch").AsBoolean().NotNullable();

            Create.TableForModel("DownloadClientStatus")
                .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTime().Nullable()
                .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTime().Nullable();
        }
    }
}
