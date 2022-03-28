using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(141)]
    public class fix_duplicate_alt_titles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(RemoveDuplicateAlternateTitles);
            Alter.Table("AlternativeTitles").AlterColumn("CleanTitle").AsString().Unique();
        }

        private void RemoveDuplicateAlternateTitles(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "DELETE FROM \"AlternativeTitles\" WHERE \"Id\" NOT IN (Select Min(\"Id\") From \"AlternativeTitles\" Group By \"CleanTitle\")";

                cmd.ExecuteNonQuery();
            }
        }
    }
}
