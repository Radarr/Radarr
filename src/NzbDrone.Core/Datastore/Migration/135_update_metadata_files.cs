using FluentMigrator;
//using FluentMigrator.Expressions;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(135)]
    public class update_metadata_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {

            // ExtraFiles SeriesId, SeasonNumber, EpisodeFileId -> nullable
            Alter.Table("ExtraFiles").AddColumn("MovieId").AsInt32().NotNullable();
            Alter.Table("ExtraFiles").AddColumn("MovieFileId").AsInt32().Nullable();

            // update series
            Alter.Table("ExtraFiles").AlterColumn("SeriesId").AsInt32().Nullable();
            Alter.Table("ExtraFiles").AlterColumn("SeasonNumber").AsInt32().Nullable();
            Alter.Table("ExtraFiles").AlterColumn("EpisodeFileId").AsInt32().Nullable();

            Alter.Table("MetadataFiles").AddColumn("MovieId").AsInt32().NotNullable();
            Alter.Table("MetadataFiles").AddColumn("MovieFileId").AsInt32().Nullable();

            // update series
            Alter.Table("MetadataFiles").AlterColumn("SeriesId").AsInt32().Nullable();
            Alter.Table("MetadataFiles").AlterColumn("SeasonNumber").AsInt32().Nullable();
            Alter.Table("MetadataFiles").AlterColumn("EpisodeFileId").AsInt32().Nullable();

            Alter.Table("SubtitleFiles").AddColumn("MovieId").AsInt32().NotNullable();
            Alter.Table("SubtitleFiles").AddColumn("MovieFileId").AsInt32().Nullable();

            // update series
            Alter.Table("SubtitleFiles").AlterColumn("SeriesId").AsInt32().Nullable();
            Alter.Table("SubtitleFiles").AlterColumn("SeasonNumber").AsInt32().Nullable();
            Alter.Table("SubtitleFiles").AlterColumn("EpisodeFileId").AsInt32().Nullable();

            // We can migrate these back in over time
            Delete.FromTable("Metadata").Row(new { Implementation = "WdtvMetadata" });
            Delete.FromTable("Metadata").Row(new { Implementation = "RoksboxMetadata" });
            Delete.FromTable("Metadata").Row(new { Implementation = "MediaBrowserMetadata" });
        }
    }
}
