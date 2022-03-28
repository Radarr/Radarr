using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(170)]
    public class fix_trakt_list_config : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public fix_trakt_list_config()
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
        }

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(FixTraktConfig);
            Execute.WithConnection(RenameRadarrListType);
            Execute.Sql("DELETE FROM \"Config\" WHERE \"Key\" IN ('TraktAuthToken', 'TraktRefreshToken', 'TraktTokenExpiry', 'NewTraktAuthToken', 'NewTraktRefreshToken', 'NewTraktTokenExpiry')");
        }

        private void RenameRadarrListType(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<ProviderDefinition169>($"SELECT \"Id\", \"Implementation\", \"ConfigContract\", \"Settings\" FROM \"NetImport\" WHERE \"Implementation\" = 'RadarrLists'");

            var corrected = new List<ProviderDefinition169>();

            foreach (var row in rows)
            {
                corrected.Add(new ProviderDefinition169
                {
                    Id = row.Id,
                    Implementation = "RadarrListImport",
                    ConfigContract = "RadarrListSettings"
                });
            }

            var updateSql = "UPDATE \"NetImport\" SET \"Implementation\" = @Implementation, \"ConfigContract\" = @ConfigContract WHERE \"Id\" = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private void FixTraktConfig(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<ProviderDefinition169>($"SELECT \"Id\", \"Implementation\", \"ConfigContract\", \"Settings\" FROM \"NetImport\" WHERE \"Implementation\" = 'TraktImport'");

            var corrected = new List<ProviderDefinition169>();

            foreach (var row in rows)
            {
                var settings = JsonSerializer.Deserialize<TraktSettings169>(row.Settings, _serializerSettings);

                if (settings.TraktListType == (int)TraktListType169.UserCustomList)
                {
                    var newSettings = new TraktListSettings170
                    {
                        Listname = settings.Listname,
                        Username = settings.Username,
                        AuthUser = settings.Username,

                        Scope = settings.Scope,
                        AccessToken = settings.AccessToken.IsNotNullOrWhiteSpace() ? settings.AccessToken : "",
                        RefreshToken = settings.RefreshToken.IsNotNullOrWhiteSpace() ? settings.RefreshToken : "",
                        Expires = settings.Expires > DateTime.UtcNow ? settings.Expires : DateTime.UtcNow,
                        Link = settings.Link,
                        Rating = settings.Rating,
                        Certification = settings.Certification,
                        Genres = settings.Genres,
                        Years = settings.Years,
                        Limit = settings.Limit,
                        TraktAdditionalParameters = settings.TraktAdditionalParameters,
                        SignIn = settings.SignIn
                    };

                    corrected.Add(new ProviderDefinition169
                    {
                        Id = row.Id,
                        Implementation = "TraktListImport",
                        ConfigContract = "TraktListSettings",
                        Settings = JsonSerializer.Serialize(newSettings, _serializerSettings)
                    });
                }
                else if (settings.TraktListType == (int)TraktListType169.UserWatchedList || settings.TraktListType == (int)TraktListType169.UserWatchList)
                {
                    var newSettings = new TraktUserSettings170
                    {
                        TraktListType = settings.TraktListType,
                        AuthUser = settings.Username,

                        Scope = settings.Scope,
                        AccessToken = settings.AccessToken.IsNotNullOrWhiteSpace() ? settings.AccessToken : "",
                        RefreshToken = settings.RefreshToken.IsNotNullOrWhiteSpace() ? settings.RefreshToken : "",
                        Expires = settings.Expires > DateTime.UtcNow ? settings.Expires : DateTime.UtcNow,
                        Link = settings.Link,
                        Rating = settings.Rating,
                        Certification = settings.Certification,
                        Genres = settings.Genres,
                        Years = settings.Years,
                        Limit = settings.Limit,
                        TraktAdditionalParameters = settings.TraktAdditionalParameters,
                        SignIn = settings.SignIn
                    };

                    corrected.Add(new ProviderDefinition169
                    {
                        Id = row.Id,
                        Implementation = "TraktUserImport",
                        ConfigContract = "TraktUserSettings",
                        Settings = JsonSerializer.Serialize(newSettings, _serializerSettings)
                    });
                }
                else
                {
                    var newSettings = new TraktPopularSettings170
                    {
                        TraktListType = (int)Enum.Parse(typeof(TraktPopularListType170), Enum.GetName(typeof(TraktListType169), settings.TraktListType)),
                        AuthUser = settings.Username,

                        Scope = settings.Scope,
                        AccessToken = settings.AccessToken.IsNotNullOrWhiteSpace() ? settings.AccessToken : "",
                        RefreshToken = settings.RefreshToken.IsNotNullOrWhiteSpace() ? settings.RefreshToken : "",
                        Expires = settings.Expires > DateTime.UtcNow ? settings.Expires : DateTime.UtcNow,
                        Link = settings.Link,
                        Rating = settings.Rating,
                        Certification = settings.Certification,
                        Genres = settings.Genres,
                        Years = settings.Years,
                        Limit = settings.Limit,
                        TraktAdditionalParameters = settings.TraktAdditionalParameters,
                        SignIn = settings.SignIn
                    };

                    corrected.Add(new ProviderDefinition169
                    {
                        Id = row.Id,
                        Implementation = "TraktPopularImport",
                        ConfigContract = "TraktPopularSettings",
                        Settings = JsonSerializer.Serialize(newSettings, _serializerSettings)
                    });
                }
            }

            Console.WriteLine(corrected.ToJson());

            var updateSql = "UPDATE \"NetImport\" SET \"Implementation\" = @Implementation, \"ConfigContract\" = @ConfigContract, \"Settings\" = @Settings WHERE \"Id\" = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }
    }

    public class ProviderDefinition169 : ModelBase
    {
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public string Settings { get; set; }
    }

    public class TraktBaseSettings170
    {
        public string Scope { get; set; }
        public string AuthUser { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expires { get; set; }
        public string Link { get; set; }
        public string Rating { get; set; }
        public string Certification { get; set; }
        public string Genres { get; set; }
        public string Years { get; set; }
        public int Limit { get; set; }
        public string TraktAdditionalParameters { get; set; }
        public string SignIn { get; set; }
    }

    public class TraktListSettings170 : TraktBaseSettings170
    {
        public string Username { get; set; }
        public string Listname { get; set; }
    }

    public class TraktPopularSettings170 : TraktBaseSettings170
    {
        public int TraktListType { get; set; }
    }

    public class TraktUserSettings170 : TraktBaseSettings170
    {
        public int TraktListType { get; set; }
    }

    public class TraktSettings169
    {
        public string OAuthUrl { get; set; }
        public string RenewUri { get; set; }
        public string ClientId { get; set; }
        public virtual string Scope { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expires { get; set; }
        public string Link { get; set; }
        public int TraktListType { get; set; }
        public string Username { get; set; }
        public string Listname { get; set; }
        public string Rating { get; set; }
        public string Certification { get; set; }
        public string Genres { get; set; }
        public string Years { get; set; }
        public int Limit { get; set; }
        public string TraktAdditionalParameters { get; set; }
        public string SignIn { get; set; }
    }

    public enum TraktListType169
    {
        UserWatchList = 0,
        UserWatchedList = 1,
        UserCustomList = 2,

        Trending = 3,
        Popular = 4,
        Anticipated = 5,
        BoxOffice = 6,

        TopWatchedByWeek = 7,
        TopWatchedByMonth = 8,
        TopWatchedByYear = 9,
        TopWatchedByAllTime = 10
    }

    public enum TraktUserListType170
    {
        UserWatchList = 0,
        UserWatchedList = 1,
        UserCollectionList = 2,
    }

    public enum TraktPopularListType170
    {
        Trending = 0,
        Popular = 1,
        Anticipated = 2,
        BoxOffice = 3,

        TopWatchedByWeek = 4,
        TopWatchedByMonth = 5,
        TopWatchedByYear = 6,
        TopWatchedByAllTime = 7
    }
}
