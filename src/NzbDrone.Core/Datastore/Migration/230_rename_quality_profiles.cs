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

            if (Schema.Table("Movies").Column("ProfileId").Exists())
            {
                Rename.Column("ProfileId").OnTable("Movies").To("QualityProfileId");
            }

            if (Schema.Table("ImportLists").Column("ProfileId").Exists())
            {
                Rename.Column("ProfileId").OnTable("ImportLists").To("QualityProfileId");
            }
        }
    }
}
