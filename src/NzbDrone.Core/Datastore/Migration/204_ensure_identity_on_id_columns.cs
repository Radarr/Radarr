using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(204)]
    public class ensure_identity_on_id_columns : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Purge Commands before reworking tables
            Delete.FromTable("Commands").AllRows();

            Alter.Column("Id").OnTable("Movies").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("MovieTranslations").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("Commands").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("Credits").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("Profiles").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("PendingReleases").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("NamingConfig").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("History").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("Blocklist").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("MovieFiles").AsInt32().PrimaryKey().Identity();
            Alter.Column("Id").OnTable("CustomFormats").AsInt32().PrimaryKey().Identity();
        }
    }
}
