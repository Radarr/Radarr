using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(117)]
    public class update_movie_file : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Column("Edition").OnTable("MovieFiles").AsString().Nullable();

            //Execute.WithConnection(SetSortTitles);
        }

        private void SetSortTitles(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT ""Id"", ""RelativePath"" FROM ""MovieFiles""";
                using (IDataReader seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var id = seriesReader.GetInt32(0);
                        var relativePath = seriesReader.GetString(1);

                        var result = Parser.Parser.ParseMovieTitle(relativePath);

                        var edition = "";

                        if (result != null)
                        {
                            edition = result.Edition ?? Parser.Parser.ParseEdition(result.SimpleReleaseTitle);
                        }

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE \"MovieFiles\" SET \"Edition\" = ? WHERE \"Id\" = ?";
                            updateCmd.AddParameter(edition);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
