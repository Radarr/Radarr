using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(137)]
    public class add_import_exclusions_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!this.Schema.Schema("dbo").Table("ImportExclusions").Exists())
            {
                Create.Table("ImportExclusions").WithColumn("tmdbid").AsInt64().NotNullable().Unique().PrimaryKey();
            }
            Execute.WithConnection(AddExisting);
        }

        private void AddExisting(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT Key, Value FROM Config WHERE Key = 'importexclusions'";
                using (IDataReader seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var Key = seriesReader.GetString(0);
                        var Value = seriesReader.GetString(1);
                        var importExclusions = Value.Split(',').Select(x => "(\""+Regex.Replace(x, @"^.*\-(.*)$", "$1")+"\")").ToList();

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "INSERT INTO ImportExclusions (tmdbid) VALUES " + string.Join(", ", importExclusions);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}