using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(216)]
    public class clean_alt_titles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Language").FromTable("AlternativeTitles");
            Delete.Column("Votes").FromTable("AlternativeTitles");
            Delete.Column("VoteCount").FromTable("AlternativeTitles");
            Delete.Column("SourceId").FromTable("AlternativeTitles");
        }
    }
}
