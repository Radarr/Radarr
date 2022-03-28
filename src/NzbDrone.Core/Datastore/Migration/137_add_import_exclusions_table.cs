using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(137)]
    public class add_import_exclusions_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!Schema.Schema("dbo").Table("ImportExclusions").Exists())
            {
                Create.TableForModel("ImportExclusions")
                    .WithColumn("TmdbId").AsInt64().NotNullable().Unique().PrimaryKey()
                    .WithColumn("MovieTitle").AsString().Nullable()
                    .WithColumn("MovieYear").AsInt64().Nullable().WithDefaultValue(0);
            }

            Execute.WithConnection(AddExisting);
        }

        private void AddExisting(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT ""Key"", ""Value"" FROM ""Config"" WHERE ""Key"" = 'importexclusions'";
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                using (IDataReader seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var key = seriesReader.GetString(0);
                        var value = seriesReader.GetString(1);

                        var importExclusions = value.Split(',').Select(x =>
                        {
                            return string.Format("(\"{0}\", \"{1}\")",
                                Regex.Replace(x, @"^.*\-(.*)$", "$1"),
                                textInfo.ToTitleCase(string.Join(" ", x.Split('-').DropLast(1))));
                        }).ToList();

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "INSERT INTO \"ImportExclusions\" (tmdbid, MovieTitle) VALUES " + string.Join(", ", importExclusions);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
