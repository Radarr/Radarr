using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(246)]
    public class add_books_audiobooks_tables : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Books")
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("ForeignBookId").AsString().Nullable()
                .WithColumn("Isbn").AsString().Nullable()
                .WithColumn("Isbn13").AsString().Nullable()
                .WithColumn("Asin").AsString().Nullable()
                .WithColumn("PageCount").AsInt32().Nullable()
                .WithColumn("ReleaseDate").AsDateTime().Nullable()
                .WithColumn("Publisher").AsString().Nullable()
                .WithColumn("Language").AsString().Nullable()
                .WithColumn("MediaType").AsInt32().WithDefaultValue(4)
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("QualityProfileId").AsInt32().WithDefaultValue(0)
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("RootFolderPath").AsString().Nullable()
                .WithColumn("Added").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("Tags").AsString().WithDefaultValue("[]")
                .WithColumn("LastSearchTime").AsDateTime().Nullable()
                .WithColumn("AuthorId").AsInt32().Nullable()
                .WithColumn("SeriesId").AsInt32().Nullable()
                .WithColumn("SeriesPosition").AsInt32().Nullable();

            Create.TableForModel("Audiobooks")
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("ForeignAudiobookId").AsString().Nullable()
                .WithColumn("Isbn").AsString().Nullable()
                .WithColumn("Isbn13").AsString().Nullable()
                .WithColumn("Asin").AsString().Nullable()
                .WithColumn("ReleaseDate").AsDateTime().Nullable()
                .WithColumn("Publisher").AsString().Nullable()
                .WithColumn("Language").AsString().Nullable()
                .WithColumn("Narrator").AsString().Nullable()
                .WithColumn("DurationMinutes").AsInt32().Nullable()
                .WithColumn("IsAbridged").AsBoolean().WithDefaultValue(false)
                .WithColumn("MediaType").AsInt32().WithDefaultValue(5)
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("QualityProfileId").AsInt32().WithDefaultValue(0)
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("RootFolderPath").AsString().Nullable()
                .WithColumn("Added").AsDateTime().WithDefaultValue(System.DateTime.UtcNow)
                .WithColumn("Tags").AsString().WithDefaultValue("[]")
                .WithColumn("LastSearchTime").AsDateTime().Nullable()
                .WithColumn("AuthorId").AsInt32().Nullable()
                .WithColumn("SeriesId").AsInt32().Nullable()
                .WithColumn("SeriesPosition").AsInt32().Nullable()
                .WithColumn("BookId").AsInt32().Nullable();
        }
    }
}
