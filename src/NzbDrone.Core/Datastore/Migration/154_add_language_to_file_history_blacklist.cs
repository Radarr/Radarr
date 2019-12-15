using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Languages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Datastore.Migration
{
    // this is here to resolve ambiguity in GetValueOrDefault extension method in net core 3
    using NzbDrone.Common.Extensions;

    [Migration(154)]
    public class add_language_to_files_history_blacklist : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("MovieFiles")
                 .AddColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            Alter.Table("History")
                 .AddColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            Alter.Table("Blacklist")
                 .AddColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            Execute.WithConnection(UpdateLanguage);
        }

        private void UpdateLanguage(IDbConnection conn, IDbTransaction tran)
        {
            var LanguageConverter = new EmbeddedDocumentConverter<List<Language>>(new LanguageIntConverter());

            var profileLanguages = new Dictionary<int, int>();
            using (IDbCommand getProfileCmd = conn.CreateCommand())
            {
                getProfileCmd.Transaction = tran;
                getProfileCmd.CommandText = "SELECT Id, Language FROM Profiles";

                IDataReader profilesReader = getProfileCmd.ExecuteReader();
                while (profilesReader.Read())
                {
                    var profileId = profilesReader.GetInt32(0);
                    var movieLanguage = Language.English.Id;
                    try
                    {
                        movieLanguage = profilesReader.GetInt32(1);
                    }
                    catch (InvalidCastException e)
                    {
                        _logger.Debug("Language field not found in Profiles, using English as default." + e.Message);
                    }

                    profileLanguages[profileId] = movieLanguage;
                }
            }

            var movieLanguages = new Dictionary<int, int>();

            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT Id, ProfileId FROM Movies";
                using (IDataReader moviesReader = getSeriesCmd.ExecuteReader())
                {
                    while (moviesReader.Read())
                    {
                        var movieId = moviesReader.GetInt32(0);
                        var movieProfileId = moviesReader.GetInt32(1);

                        movieLanguages[movieId] = profileLanguages.GetValueOrDefault(movieProfileId, Language.English.Id);
                    }
                }
            }

            foreach (var group in movieLanguages.GroupBy(v => v.Value, v => v.Key))
            {
                var language = new List<Language> { Language.FindById(group.Key) };

                var movieIds = group.Select(v => v.ToString()).Join(",");

                using (IDbCommand updateMovieFilesCmd = conn.CreateCommand())
                {
                    updateMovieFilesCmd.Transaction = tran;
                    updateMovieFilesCmd.CommandText = $"UPDATE MovieFiles SET Languages = ? WHERE MovieId IN ({movieIds})";
                    var param = updateMovieFilesCmd.CreateParameter();
                    LanguageConverter.SetValue(param, language);

                    updateMovieFilesCmd.ExecuteNonQuery();
                }

                using (IDbCommand updateHistoryCmd = conn.CreateCommand())
                {
                    updateHistoryCmd.Transaction = tran;
                    updateHistoryCmd.CommandText = $"UPDATE History SET Languages = ? WHERE MovieId IN ({movieIds})";
                    var param = updateHistoryCmd.CreateParameter();
                    LanguageConverter.SetValue(param, language);

                    updateHistoryCmd.ExecuteNonQuery();
                }

                using (IDbCommand updateBlacklistCmd = conn.CreateCommand())
                {
                    updateBlacklistCmd.Transaction = tran;
                    updateBlacklistCmd.CommandText = $"UPDATE Blacklist SET Languages = ? WHERE MovieId IN ({movieIds})";
                    var param = updateBlacklistCmd.CreateParameter();
                    LanguageConverter.SetValue(param, language);

                    updateBlacklistCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
