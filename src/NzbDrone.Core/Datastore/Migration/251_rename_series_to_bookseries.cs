using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(251)]
    public class rename_series_to_bookseries : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Series").To("BookSeries");

            Rename.Column("SeriesId").OnTable("Books").To("BookSeriesId");
            Rename.Column("SeriesId").OnTable("Audiobooks").To("BookSeriesId");

            Delete.Index("IX_Books_SeriesId_Monitored").OnTable("Books");
            Delete.Index("IX_Audiobooks_SeriesId_Monitored").OnTable("Audiobooks");

            Create.Index("IX_Books_BookSeriesId_Monitored")
                .OnTable("Books")
                .OnColumn("BookSeriesId").Ascending()
                .OnColumn("Monitored").Ascending();

            Create.Index("IX_Audiobooks_BookSeriesId_Monitored")
                .OnTable("Audiobooks")
                .OnColumn("BookSeriesId").Ascending()
                .OnColumn("Monitored").Ascending();
        }
    }
}
