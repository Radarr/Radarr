using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(248)]
    public class add_hierarchical_monitoring_indexes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index("IX_Series_AuthorId_Monitored")
                .OnTable("Series")
                .OnColumn("AuthorId").Ascending()
                .OnColumn("Monitored").Ascending();

            Create.Index("IX_Books_AuthorId_Monitored")
                .OnTable("Books")
                .OnColumn("AuthorId").Ascending()
                .OnColumn("Monitored").Ascending();

            Create.Index("IX_Books_SeriesId_Monitored")
                .OnTable("Books")
                .OnColumn("SeriesId").Ascending()
                .OnColumn("Monitored").Ascending();

            Create.Index("IX_Audiobooks_AuthorId_Monitored")
                .OnTable("Audiobooks")
                .OnColumn("AuthorId").Ascending()
                .OnColumn("Monitored").Ascending();

            Create.Index("IX_Audiobooks_SeriesId_Monitored")
                .OnTable("Audiobooks")
                .OnColumn("SeriesId").Ascending()
                .OnColumn("Monitored").Ascending();

            Create.Index("IX_Audiobooks_BookId_Monitored")
                .OnTable("Audiobooks")
                .OnColumn("BookId").Ascending()
                .OnColumn("Monitored").Ascending();
        }
    }
}
