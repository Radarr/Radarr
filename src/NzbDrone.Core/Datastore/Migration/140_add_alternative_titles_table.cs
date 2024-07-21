using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(140)]
    public class add_alternative_titles_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!Schema.Table("AlternativeTitles").Exists())
            {
                Create.TableForModel("AlternativeTitles")
                      .WithColumn("MovieId").AsInt64().NotNullable()
                      .WithColumn("Title").AsString().NotNullable()
                      .WithColumn("CleanTitle").AsString().NotNullable()
                      .WithColumn("SourceType").AsInt64().WithDefaultValue(0)
                      .WithColumn("SourceId").AsInt64().WithDefaultValue(0)
                      .WithColumn("Votes").AsInt64().WithDefaultValue(0)
                      .WithColumn("VoteCount").AsInt64().WithDefaultValue(0)
                      .WithColumn("Language").AsInt64().WithDefaultValue(0);

                Delete.Column("AlternativeTitles").FromTable("Movies");
            }

            Alter.Table("Movies").AddColumn("SecondaryYear").AsInt32().Nullable();
            Alter.Table("Movies").AddColumn("SecondaryYearSourceId").AsInt64().Nullable().WithDefaultValue(0);
        }
    }
}
