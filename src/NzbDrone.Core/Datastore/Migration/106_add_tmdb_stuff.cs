using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(106)]
    public class add_tmdb_stuff : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies")
                  .AddColumn("TmdbId").AsInt32().WithDefaultValue(0);
            Alter.Table("Movies")
                .AddColumn("Website").AsString().Nullable();
            Alter.Table("Movies")
                .AlterColumn("ImdbId").AsString().Nullable();
            Alter.Table("Movies")
                .AddColumn("AlternativeTitles").AsString().Nullable();
        }
    }
}
