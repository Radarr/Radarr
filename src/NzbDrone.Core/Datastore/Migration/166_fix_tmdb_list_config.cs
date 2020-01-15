using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(166)]
    public class fix_tmdb_list_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(FixConfig);
        }

        private void FixConfig(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<ProviderDefinition166>($"SELECT Id, Implementation, ConfigContract, Settings FROM NetImport WHERE Implementation = 'TMDbImport'");

            var corrected = new List<ProviderDefinition166>();

            var serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            foreach (var row in rows)
            {
                var settings = JsonSerializer.Deserialize<TMDbSettings165>(row.Settings, serializerSettings);

                if (settings.ListId.IsNotNullOrWhiteSpace())
                {
                    var newSettings = new TMDbListSettings166
                    {
                        ListId = settings.ListId
                    };

                    corrected.Add(new ProviderDefinition166
                    {
                        Id = row.Id,
                        Implementation = "TMDbListImport",
                        ConfigContract = "TMDbListSettings",
                        Settings = JsonSerializer.Serialize(newSettings, serializerSettings)
                    });
                }
                else
                {
                    var newSettings = new TMDbPopularSettings166
                    {
                        ListType = settings.ListType,
                        FilterCriteria = new TMDbFilterSettings166
                        {
                            MinVoteAverage = settings.MinVoteAverage,
                            MinVotes = settings.MinVotes,
                            Ceritification = settings.Ceritification,
                            IncludeGenreIds = settings.IncludeGenreIds,
                            ExcludeGenreIds = settings.ExcludeGenreIds,
                            LanguageCode = settings.LanguageCode
                        }
                    };

                    corrected.Add(new ProviderDefinition166
                    {
                        Id = row.Id,
                        Implementation = "TMDbPopularImport",
                        ConfigContract = "TMDbPopularSettings",
                        Settings = JsonSerializer.Serialize(newSettings, serializerSettings)
                    });
                }
            }

            Console.WriteLine(corrected.ToJson());

            var updateSql = "UPDATE NetImport SET Implementation = @Implementation, ConfigContract = @ConfigContract, Settings = @Settings WHERE Id = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private class ProviderDefinition166 : ModelBase
        {
            public string Implementation { get; set; }
            public string ConfigContract { get; set; }
            public string Settings { get; set; }
        }

        private class TMDbSettings165
        {
            public string Link { get; set; }
            public int ListType { get; set; }
            public string ListId { get; set; }
            public string MinVoteAverage { get; set; }
            public string MinVotes { get; set; }
            public string Ceritification { get; set; }
            public string IncludeGenreIds { get; set; }
            public string ExcludeGenreIds { get; set; }
            public int LanguageCode { get; set; }
        }

        private class TMDbListSettings166
        {
            public string ListId { get; set; }
        }

        private class TMDbPopularSettings166
        {
            public int ListType { get; set; }
            public TMDbFilterSettings166 FilterCriteria { get; set; }
        }

        private class TMDbFilterSettings166
        {
            public string MinVoteAverage { get; set; }
            public string MinVotes { get; set; }
            public string Ceritification { get; set; }
            public string IncludeGenreIds { get; set; }
            public string ExcludeGenreIds { get; set; }
            public int LanguageCode { get; set; }
        }
    }
}
