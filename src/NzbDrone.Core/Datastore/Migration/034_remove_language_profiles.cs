using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(34)]
    public class remove_language_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Table("LanguageProfiles");

            Delete.Column("LanguageProfileId").FromTable("Artists");
            Delete.Column("LanguageProfileId").FromTable("ImportLists");
            Delete.Column("Language").FromTable("Blacklist");
            Delete.Column("Language").FromTable("History");
            Delete.Column("Language").FromTable("LyricFiles");
            Delete.Column("Language").FromTable("TrackFiles");
        }
    }
}
