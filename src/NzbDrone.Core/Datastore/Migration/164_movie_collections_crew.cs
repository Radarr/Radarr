using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(164)]
    public class movie_collections_crew : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies").AddColumn("Collection").AsString().Nullable();
            Delete.Column("Actors").FromTable("Movies");

            Create.TableForModel("Credits").WithColumn("MovieId").AsInt32()
                                  .WithColumn("CreditTmdbId").AsString().Unique()
                                  .WithColumn("PersonTmdbId").AsInt32()
                                  .WithColumn("Name").AsString()
                                  .WithColumn("Images").AsString()
                                  .WithColumn("Character").AsString().Nullable()
                                  .WithColumn("Order").AsInt32()
                                  .WithColumn("Job").AsString().Nullable()
                                  .WithColumn("Department").AsString().Nullable()
                                  .WithColumn("Type").AsInt32();

            Create.Index().OnTable("Credits").OnColumn("MovieId");

            Delete.FromTable("Notifications").Row(new { Implementation = "NotifyMyAndroid" });
            Delete.FromTable("Notifications").Row(new { Implementation = "Pushalot" });
        }
    }
}
