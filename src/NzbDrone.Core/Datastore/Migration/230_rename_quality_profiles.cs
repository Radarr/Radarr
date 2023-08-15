using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(230)]
    public class rename_quality_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Profiles").To("QualityProfiles");
            Rename.Column("ProfileId").OnTable("Movies").To("QualityProfileId");
            Rename.Column("ProfileId").OnTable("ImportLists").To("QualityProfileId");
        }
    }
}
