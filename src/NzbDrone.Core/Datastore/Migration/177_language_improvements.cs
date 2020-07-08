using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(177)]
    public class language_improvements : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public language_improvements()
        {
            _serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        protected override void MainDbUpgrade()
        {
            // Use original language to set default language fallback for releases
            // Set all to English (1) on migration to ensure default behavior persists until refresh
            Alter.Table("Movies").AddColumn("OriginalLanguage").AsInt32().WithDefaultValue((int)Language.English);
            Alter.Table("Movies").AddColumn("OriginalTitle").AsString().Nullable();

            Alter.Table("Movies").AddColumn("DigitalRelease").AsDateTime().Nullable();

            // Column not used
            Delete.Column("PhysicalReleaseNote").FromTable("Movies");
            Delete.Column("SecondaryYearSourceId").FromTable("Movies");

            Alter.Table("NamingConfig").AddColumn("RenameMovies").AsBoolean().WithDefaultValue(0);
            Execute.Sql("UPDATE NamingConfig SET RenameMovies=RenameEpisodes");
            Delete.Column("RenameEpisodes").FromTable("NamingConfig");

            //Manual SQL, Fluent Migrator doesn't support multi-column unique contraint on table creation, SQLite doesn't support adding it after creation
            Execute.Sql("CREATE TABLE MovieTranslations(" +
                "Id INTEGER PRIMARY KEY, " +
                "MovieId INTEGER NOT NULL, " +
                "Title TEXT, " +
                "CleanTitle TEXT, " +
                "Overview TEXT, " +
                "Language INTEGER NOT NULL, " +
                "Unique(\"MovieId\", \"Language\"));");

            // Prevent failure if two movies have same alt titles
            Execute.Sql("DROP INDEX IF EXISTS \"IX_AlternativeTitles_CleanTitle\"");

            Execute.WithConnection(FixLanguagesMoveFile);
            Execute.WithConnection(FixLanguagesHistory);
        }

        private void FixLanguagesMoveFile(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<LanguageEntity177>($"SELECT Id, Languages FROM MovieFiles");

            var corrected = new List<LanguageEntity177>();

            foreach (var row in rows)
            {
                var languages = JsonSerializer.Deserialize<List<int>>(row.Languages, _serializerSettings);

                var newLanguages = languages.Distinct().ToList();

                corrected.Add(new LanguageEntity177
                {
                    Id = row.Id,
                    Languages = JsonSerializer.Serialize(newLanguages, _serializerSettings)
                });
            }

            var updateSql = "UPDATE MovieFiles SET Languages = @Languages WHERE Id = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private void FixLanguagesHistory(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<LanguageEntity177>($"SELECT Id, Languages FROM History");

            var corrected = new List<LanguageEntity177>();

            foreach (var row in rows)
            {
                var languages = JsonSerializer.Deserialize<List<int>>(row.Languages, _serializerSettings);

                var newLanguages = languages.Distinct().ToList();

                corrected.Add(new LanguageEntity177
                {
                    Id = row.Id,
                    Languages = JsonSerializer.Serialize(newLanguages, _serializerSettings)
                });
            }

            var updateSql = "UPDATE History SET Languages = @Languages WHERE Id = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private class LanguageEntity177 : ModelBase
        {
            public string Languages { get; set; }
        }
    }
}
