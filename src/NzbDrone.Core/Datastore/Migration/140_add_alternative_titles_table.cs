using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using Marr.Data.QGen;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(140)]
    public class add_alternative_titles_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!this.Schema.Schema("dbo").Table("alternative_titles").Exists())
            {
                Create.TableForModel("AlternativeTitles")
                      .WithColumn("MovieId").AsInt64().NotNullable()
                      .WithColumn("Title").AsString().NotNullable()
                      .WithColumn("CleanTitle").AsString().NotNullable()
                      .WithColumn("SourceType").AsInt64().WithDefault(0)
                      .WithColumn("SourceId").AsInt64().WithDefault(0)
                      .WithColumn("Votes").AsInt64().WithDefault(0)
                      .WithColumn("VoteCount").AsInt64().WithDefault(0)
                      .WithColumn("Language").AsInt64().WithDefault(0);

                Delete.Column("AlternativeTitles").FromTable("Movies");
            }

            Alter.Table("Movies").AddColumn("SecondaryYear").AsInt32().Nullable();
            Alter.Table("Movies").AddColumn("SecondaryYearSourceId").AsInt64().Nullable().WithDefault(0);
            
            Execute.WithConnection(AddExisting);
        }

        private void AddExisting(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT Key, Value FROM Config WHERE Key = 'importexclusions'";
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                using (IDataReader seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var Key = seriesReader.GetString(0);
                        var Value = seriesReader.GetString(1);

                        var importExclusions = Value.Split(',').Select(x => {
                            return string.Format("(\"{0}\", \"{1}\")", Regex.Replace(x, @"^.*\-(.*)$", "$1"),
                                                 textInfo.ToTitleCase(string.Join(" ", x.Split('-').DropLast(1))));
                        }).ToList();

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "INSERT INTO ImportExclusions (tmdbid, MovieTitle) VALUES " + string.Join(", ", importExclusions);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}