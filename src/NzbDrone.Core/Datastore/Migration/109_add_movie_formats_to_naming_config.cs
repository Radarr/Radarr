using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(109)]
    public class add_movie_formats_to_naming_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("StandardMovieFormat").AsString().Nullable();
            Alter.Table("NamingConfig").AddColumn("MovieFolderFormat").AsString().Nullable();

            Execute.WithConnection(ConvertConfig);
        }

        private void ConvertConfig(IDbConnection conn, IDbTransaction tran)
        {

            using (IDbCommand namingConfigCmd = conn.CreateCommand())
            {
                namingConfigCmd.Transaction = tran;
                namingConfigCmd.CommandText = @"SELECT * FROM NamingConfig LIMIT 1";
                using (IDataReader namingConfigReader = namingConfigCmd.ExecuteReader())
                {
                    var separatorIndex = namingConfigReader.GetOrdinal("Separator");
                    var includeQualityIndex = namingConfigReader.GetOrdinal("IncludeQuality");
                    var replaceSpacesIndex = namingConfigReader.GetOrdinal("ReplaceSpaces");

                    while (namingConfigReader.Read())
                    {
                        var separator = namingConfigReader.GetString(separatorIndex);
                        var includeQuality = namingConfigReader.GetBoolean(includeQualityIndex);
                        var replaceSpaces = namingConfigReader.GetBoolean(replaceSpacesIndex);

                        // Output Settings
                        var movieTitlePattern = "";
                        //var movieYearPattern = "({Release Year})";
                        var qualityFormat = " [{Quality Title}]";

                        if (replaceSpaces)
                        {
                            movieTitlePattern = "{Movie.Title}";

                        }
                        else
                        {
                            movieTitlePattern = "{Movie Title}";
                        }

                        movieTitlePattern += separator;

                        var standardMovieFormat = string.Format("{0}{1}", movieTitlePattern,
                                                                                         qualityFormat);

                        var movieFolderFormat = string.Format("{0}", movieTitlePattern);

                        if (includeQuality)
                        {
                            if (replaceSpaces)
                            {
                                qualityFormat = ".[{Quality.Title}]";
                            }

                            movieFolderFormat += qualityFormat;
                            standardMovieFormat += qualityFormat;
                        }

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            var text = string.Format("UPDATE NamingConfig " +
                                                     "SET StandardMovieFormat = '{0}', " +
                                                     "MovieFolderFormat = '{1}'",
                                                     standardMovieFormat,
                                                     movieFolderFormat);

                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = text;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
