using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(247)]
    public class add_book_audiobook_files_tables : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("BookFiles")
                .WithColumn("BookId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().Nullable()
                .WithColumn("Size").AsInt64().WithDefaultValue(0)
                .WithColumn("DateAdded").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("OriginalFilePath").AsString().Nullable()
                .WithColumn("SceneName").AsString().Nullable()
                .WithColumn("ReleaseGroup").AsString().Nullable()
                .WithColumn("Quality").AsString().WithDefaultValue("{}")
                .WithColumn("Format").AsString().Nullable();

            Create.TableForModel("AudiobookFiles")
                .WithColumn("AudiobookId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().Nullable()
                .WithColumn("Size").AsInt64().WithDefaultValue(0)
                .WithColumn("DateAdded").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("OriginalFilePath").AsString().Nullable()
                .WithColumn("SceneName").AsString().Nullable()
                .WithColumn("ReleaseGroup").AsString().Nullable()
                .WithColumn("Quality").AsString().WithDefaultValue("{}")
                .WithColumn("Format").AsString().Nullable()
                .WithColumn("DurationSeconds").AsInt32().Nullable()
                .WithColumn("Bitrate").AsInt32().Nullable()
                .WithColumn("SampleRate").AsInt32().Nullable()
                .WithColumn("Channels").AsInt32().Nullable();

            Create.Index("IX_BookFiles_BookId").OnTable("BookFiles").OnColumn("BookId");
            Create.Index("IX_AudiobookFiles_AudiobookId").OnTable("AudiobookFiles").OnColumn("AudiobookId");
        }
    }
}
