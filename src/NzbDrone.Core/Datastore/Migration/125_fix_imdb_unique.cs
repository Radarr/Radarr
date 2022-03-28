using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(125)]
    public class fix_imdb_unique : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(DeleteUniqueIndex);
        }

        private void DeleteUniqueIndex(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"DROP INDEX ""IX_Movies_ImdbId""";

                getSeriesCmd.ExecuteNonQuery();
            }
        }
    }
}
