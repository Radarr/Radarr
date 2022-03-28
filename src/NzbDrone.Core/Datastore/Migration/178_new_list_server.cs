using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(178)]
    public class new_list_server : NzbDroneMigrationBase
    {
        private static readonly Regex ImdbIdRegex = new Regex(@"^/*?imdb/list\?listId=(?<id>(ls|ur)\d+)/*?$",
                                                              RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly JsonSerializerOptions _serializerSettings;

        public new_list_server()
        {
            _serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            _serializerSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
        }

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(FixRadarrLists);
            Execute.WithConnection(FixStevenLuLists);
        }

        private void FixRadarrLists(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<NetImportDefinition178>($"SELECT * FROM \"NetImport\" WHERE \"ConfigContract\" = 'RadarrListSettings'");

            var radarrUrls = new List<string>
            {
                "https://api.radarr.video/v2",
                "https://staging.api.radarr.video"
            };

            foreach (var row in rows)
            {
                var settings = JsonSerializer.Deserialize<RadarrListSettings177>(row.Settings, _serializerSettings);
                object newSettings;

                if (!radarrUrls.Contains(settings.APIURL.TrimEnd('/')))
                {
                    // Combine root and path in new settings
                    newSettings = new RadarrListSettings178
                    {
                        Url = settings.APIURL.TrimEnd('/') + '/' + settings.Path.TrimStart('/')
                    };
                }
                else
                {
                    // It should be an imdb list
                    if (settings.Path == "/imdb/top250")
                    {
                        newSettings = new IMDbListSettings178
                        {
                            ListId = "top250"
                        };
                        row.ConfigContract = "IMDbListSettings";
                        row.Implementation = "IMDbListImport";
                    }
                    else if (settings.Path == "/imdb/popular")
                    {
                        newSettings = new IMDbListSettings178
                        {
                            ListId = "popular"
                        };
                        row.ConfigContract = "IMDbListSettings";
                        row.Implementation = "IMDbListImport";
                    }
                    else
                    {
                        var match = ImdbIdRegex.Match(settings.Path);
                        if (match.Success)
                        {
                            newSettings = new IMDbListSettings178
                            {
                                ListId = match.Groups["id"].Value
                            };
                            row.ConfigContract = "IMDbListSettings";
                            row.Implementation = "IMDbListImport";
                        }
                        else
                        {
                            newSettings = new RadarrListSettings178
                            {
                                Url = settings.APIURL.TrimEnd('/') + '/' + settings.Path.TrimStart('/')
                            };
                        }
                    }
                }

                row.Settings = JsonSerializer.Serialize(newSettings, _serializerSettings);
            }

            var updateSql = "UPDATE \"NetImport\" SET \"Implementation\" = @Implementation, " +
                "\"ConfigContract\" = @ConfigContract, " +
                "\"Settings\" = @Settings " +
                "WHERE \"Id\" = @Id";

            conn.Execute(updateSql, rows, transaction: tran);
        }

        private void FixStevenLuLists(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<NetImportDefinition178>($"SELECT * FROM \"NetImport\" WHERE \"ConfigContract\" = 'StevenLuSettings'");

            var updated = new List<NetImportDefinition178>();

            var scores = new[] { 5, 6, 7, 8, 50, 60, 70, 80 };

            foreach (var row in rows)
            {
                var settings = JsonSerializer.Deserialize<StevenLuSettings178>(row.Settings, _serializerSettings);

                if (settings.Link.StartsWith("https://s3.amazonaws.com/popular-movies"))
                {
                    var newSettings = new StevenLu2Settings178();

                    // convert to 2
                    if (settings.Link == "https://s3.amazonaws.com/popular-movies/movies.json")
                    {
                        newSettings.Source = (int)StevenLuSource178.Standard;
                        newSettings.MinScore = 5;
                        updated.Add(row);
                    }
                    else
                    {
                        var split = settings.Link.Split('/').Last().Split('-');
                        if (split.Length == 3 &&
                            split[0] == "movies" &&
                            Enum.TryParse(split[1], out StevenLuSource178 source) &&
                            int.TryParse(split[2], out var score) &&
                            scores.Contains(score))
                        {
                            newSettings.Source = (int)source;
                            newSettings.MinScore = source == StevenLuSource178.Imdb ? score : score / 10;
                            updated.Add(row);
                        }
                    }

                    row.ConfigContract = "StevenLu2Settings";
                    row.Implementation = "StevenLu2Import";
                    row.Settings = JsonSerializer.Serialize(newSettings, _serializerSettings);
                }
            }

            var updateSql = "UPDATE \"NetImport\" SET \"Implementation\" = @Implementation, " +
                "\"ConfigContract\" = @ConfigContract, " +
                "\"Settings\" = @Settings " +
                "WHERE \"Id\" = @Id";
            conn.Execute(updateSql, updated, transaction: tran);
        }

        public class NetImportDefinition178 : ModelBase
        {
            public bool Enabled { get; set; }
            public string Name { get; set; }
            public string Implementation { get; set; }
            public string ConfigContract { get; set; }
            public string Settings { get; set; }
            public bool EnableAuto { get; set; }
            public string RootFolderPath { get; set; }
            public bool ShouldMonitor { get; set; }
            public int ProfileId { get; set; }
            public int MinimumAvailability { get; set; }
            public string Tags { get; set; }
        }

        public class RadarrListSettings177
        {
            public string APIURL { get; set; }
            public string Path { get; set; }
        }

        public class RadarrListSettings178
        {
            public string Url { get; set; }
        }

        public class IMDbListSettings178
        {
            public string ListId { get; set; }
        }

        public class StevenLuSettings178
        {
            public string Link { get; set; }
        }

        public class StevenLu2Settings178
        {
            public int Source { get; set; }

            public int MinScore { get; set; }
        }

        public enum StevenLuSource178
        {
            Standard,
            Imdb,
            Metacritic,
            RottenTomatoes
        }
    }
}
