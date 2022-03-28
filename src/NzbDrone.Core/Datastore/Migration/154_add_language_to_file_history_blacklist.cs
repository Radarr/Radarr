using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Datastore.Migration
{
    // this is here to resolve ambiguity in GetValueOrDefault extension method in net core 3
#pragma warning disable SA1200
    using NzbDrone.Common.Extensions;
#pragma warning restore SA1200

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
            var languageConverter = new EmbeddedDocumentConverter<List<Language>>(new LanguageIntConverter());

            var profileLanguages = new Dictionary<int, int>();
            using (IDbCommand getProfileCmd = conn.CreateCommand())
            {
                getProfileCmd.Transaction = tran;
                getProfileCmd.CommandText = "SELECT \"Id\", \"Language\" FROM \"Profiles\"";

                IDataReader profilesReader = getProfileCmd.ExecuteReader();
                while (profilesReader.Read())
                {
                    var profileId = profilesReader.GetInt32(0);
                    var movieLanguage = Language.English.Id;
                    try
                    {
                        movieLanguage = profilesReader.GetInt32(1) != -1 ? profilesReader.GetInt32(1) : 1;
                    }
                    catch (InvalidCastException e)
                    {
                        _logger.Debug("Language field not found in Profiles, using English as default." + e.Message);
                    }

                    profileLanguages[profileId] = movieLanguage;
                }

                profilesReader.Close();
            }

            var movieLanguages = new Dictionary<int, int>();

            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT ""Id"", ""ProfileId"" FROM ""Movies""";
                using (IDataReader moviesReader = getSeriesCmd.ExecuteReader())
                {
                    while (moviesReader.Read())
                    {
                        var movieId = moviesReader.GetInt32(0);
                        var movieProfileId = moviesReader.GetInt32(1);

                        movieLanguages[movieId] = profileLanguages.GetValueOrDefault(movieProfileId, Language.English.Id);
                    }

                    moviesReader.Close();
                }
            }

            var movieFileLanguages = new Dictionary<int, List<Language>>();
            var releaseLanguages = new Dictionary<string, List<Language>>();

            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT ""Id"", ""MovieId"", ""SceneName"", ""MediaInfo"" FROM ""MovieFiles""";
                using (IDataReader movieFilesReader = getSeriesCmd.ExecuteReader())
                {
                    while (movieFilesReader.Read())
                    {
                        var movieFileId = movieFilesReader.GetInt32(0);
                        var movieId = movieFilesReader.GetInt32(1);
                        var movieFileSceneName = movieFilesReader.IsDBNull(2) ? null : movieFilesReader.GetString(2);
                        var movieFileMediaInfo = movieFilesReader.IsDBNull(3) ? null : Json.Deserialize<MediaInfo154>(movieFilesReader.GetString(3));
                        var languages = new List<Language>();

                        if (movieFileMediaInfo != null && movieFileMediaInfo.AudioLanguages.IsNotNullOrWhiteSpace())
                        {
                            var mediaInfolanguages = movieFileMediaInfo.AudioLanguages.Split('/').Select(l => l.Trim()).Distinct().ToList();

                            foreach (var audioLanguage in mediaInfolanguages)
                            {
                                var language = IsoLanguages.FindByName(audioLanguage)?.Language;
                                languages.AddIfNotNull(language);
                            }
                        }

                        if (!languages.Any(l => l.Id != 0) && movieFileSceneName.IsNotNullOrWhiteSpace())
                        {
                            languages = LanguageParser.ParseLanguages(movieFileSceneName);
                        }

                        if (!languages.Any(l => l.Id != 0))
                        {
                            languages = new List<Language> { Language.FindById(movieLanguages[movieId]) };
                        }

                        if (movieFileSceneName.IsNotNullOrWhiteSpace())
                        {
                            // Store languages for this scenerelease so we can use in history later
                            releaseLanguages[movieFileSceneName] = languages;
                        }

                        movieFileLanguages[movieFileId] = languages;
                    }

                    movieFilesReader.Close();
                }
            }

            var historyLanguages = new Dictionary<int, List<Language>>();

            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT ""Id"", ""SourceTitle"", ""MovieId"" FROM ""History""";
                using (IDataReader historyReader = getSeriesCmd.ExecuteReader())
                {
                    while (historyReader.Read())
                    {
                        var historyId = historyReader.GetInt32(0);
                        var historySourceTitle = historyReader.IsDBNull(1) ? null : historyReader.GetString(1);
                        var movieId = historyReader.GetInt32(2);
                        var languages = new List<Language>();

                        if (historySourceTitle.IsNotNullOrWhiteSpace() && releaseLanguages.ContainsKey(historySourceTitle))
                        {
                            languages = releaseLanguages[historySourceTitle];
                        }

                        if (!languages.Any(l => l.Id != 0) && historySourceTitle.IsNotNullOrWhiteSpace())
                        {
                            languages = LanguageParser.ParseLanguages(historySourceTitle);
                        }

                        if (!languages.Any(l => l.Id != 0))
                        {
                            languages = new List<Language> { Language.FindById(movieLanguages[movieId]) };
                        }

                        historyLanguages[historyId] = languages;
                    }

                    historyReader.Close();
                }
            }

            var blacklistLanguages = new Dictionary<int, List<Language>>();

            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT ""Id"", ""SourceTitle"", ""MovieId"" FROM ""Blacklist""";
                using (IDataReader blacklistReader = getSeriesCmd.ExecuteReader())
                {
                    while (blacklistReader.Read())
                    {
                        var blacklistId = blacklistReader.GetInt32(0);
                        var blacklistSourceTitle = blacklistReader.IsDBNull(1) ? null : blacklistReader.GetString(1);
                        var movieId = blacklistReader.GetInt32(2);
                        var languages = new List<Language>();

                        if (blacklistSourceTitle.IsNotNullOrWhiteSpace())
                        {
                            languages = LanguageParser.ParseLanguages(blacklistSourceTitle);
                        }

                        if (!languages.Any(l => l.Id != 0))
                        {
                            languages = new List<Language> { Language.FindById(movieLanguages[movieId]) };
                        }

                        blacklistLanguages[blacklistId] = languages;
                    }

                    blacklistReader.Close();
                }
            }

            foreach (var group in movieFileLanguages.GroupBy(v => v.Value, v => v.Key))
            {
                var languages = group.Key;

                var movieFileIds = group.Select(v => v.ToString()).Join(",");

                using (IDbCommand updateMovieFilesCmd = conn.CreateCommand())
                {
                    updateMovieFilesCmd.Transaction = tran;
                    if (conn.GetType().FullName == "Npgsql.NpgsqlConnection")
                    {
                        updateMovieFilesCmd.CommandText = $"UPDATE \"MovieFiles\" SET \"Languages\" = $1 WHERE \"Id\" IN ({movieFileIds})";
                    }
                    else
                    {
                        updateMovieFilesCmd.CommandText = $"UPDATE \"MovieFiles\" SET \"Languages\" = ? WHERE \"Id\" IN ({movieFileIds})";
                    }

                    var param = updateMovieFilesCmd.CreateParameter();
                    languageConverter.SetValue(param, languages);
                    updateMovieFilesCmd.Parameters.Add(param);

                    updateMovieFilesCmd.ExecuteNonQuery();
                }
            }

            foreach (var group in historyLanguages.GroupBy(v => v.Value, v => v.Key))
            {
                var languages = group.Key;

                var historyIds = group.Select(v => v.ToString()).Join(",");

                using (IDbCommand updateHistoryCmd = conn.CreateCommand())
                {
                    updateHistoryCmd.Transaction = tran;
                    if (conn.GetType().FullName == "Npgsql.NpgsqlConnection")
                    {
                        updateHistoryCmd.CommandText = $"UPDATE \"History\" SET \"Languages\" = $1 WHERE \"Id\" IN ({historyIds})";
                    }
                    else
                    {
                        updateHistoryCmd.CommandText = $"UPDATE \"History\" SET \"Languages\" = ? WHERE \"Id\" IN ({historyIds})";
                    }

                    var param = updateHistoryCmd.CreateParameter();
                    languageConverter.SetValue(param, languages);
                    updateHistoryCmd.Parameters.Add(param);

                    updateHistoryCmd.ExecuteNonQuery();
                }
            }

            foreach (var group in blacklistLanguages.GroupBy(v => v.Value, v => v.Key))
            {
                var languages = group.Key;

                var blacklistIds = group.Select(v => v.ToString()).Join(",");

                using (IDbCommand updateBlacklistCmd = conn.CreateCommand())
                {
                    updateBlacklistCmd.Transaction = tran;
                    if (conn.GetType().FullName == "Npgsql.NpgsqlConnection")
                    {
                        updateBlacklistCmd.CommandText = $"UPDATE \"Blacklist\" SET \"Languages\" = $1 WHERE \"Id\" IN ({blacklistIds})";
                    }
                    else
                    {
                        updateBlacklistCmd.CommandText = $"UPDATE \"Blacklist\" SET \"Languages\" = ? WHERE \"Id\" IN ({blacklistIds})";
                    }

                    var param = updateBlacklistCmd.CreateParameter();
                    languageConverter.SetValue(param, languages);
                    updateBlacklistCmd.Parameters.Add(param);

                    updateBlacklistCmd.ExecuteNonQuery();
                }
            }
        }
    }

    public class MediaInfo154
    {
        public string AudioLanguages { get; set; }
    }
}
