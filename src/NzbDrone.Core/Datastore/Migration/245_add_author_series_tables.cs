using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(245)]
    public class add_author_series_tables : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Authors")
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("SortName").AsString().Nullable()
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("ForeignAuthorId").AsString().Nullable()
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("RootFolderPath").AsString().Nullable()
                .WithColumn("QualityProfileId").AsInt32().WithDefaultValue(0)
                .WithColumn("Added").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("Tags").AsString().WithDefaultValue("[]");

            Create.TableForModel("Series")
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("ForeignSeriesId").AsString().Nullable()
                .WithColumn("AuthorId").AsInt32().Nullable()
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false);

            Alter.Table("Movies")
                .AddColumn("AuthorId").AsInt32().Nullable()
                .AddColumn("SeriesId").AsInt32().Nullable();
        }
    }
}
