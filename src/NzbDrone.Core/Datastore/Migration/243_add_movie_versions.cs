using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(243)]
    public class add_movie_versions : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("MovieVersions")
                .WithColumn("MovieId").AsInt32().Indexed()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("MovieFileId").AsInt32().WithDefaultValue(0);

            // 0 = file of the movie's primary version, so existing files need no update.
            Alter.Table("MovieFiles").AddColumn("MovieVersionId").AsInt32().WithDefaultValue(0);

            // Every existing movie becomes a movie with a single version.
            Execute.Sql(@"INSERT INTO ""MovieVersions"" (""MovieId"", ""QualityProfileId"", ""Path"", ""MovieFileId"")
                          SELECT ""Id"", ""QualityProfileId"", ""Path"", ""MovieFileId"" FROM ""Movies""");
        }
    }
}
