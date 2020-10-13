using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(206)]
    public class multiple_ratings_support : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public multiple_ratings_support()
        {
            _serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE CustomFilters SET Filters = Replace(Filters, 'ratings', 'tmdbRating') WHERE Type = 'discoverMovie';");
            Execute.Sql("UPDATE CustomFilters SET Filters = Replace(Filters, 'ratings', 'tmdbRating') WHERE Type = 'movieIndex';");

            Execute.WithConnection((conn, tran) => FixRatings(conn, tran, "Movies"));
            Execute.WithConnection((conn, tran) => FixRatings(conn, tran, "ImportListMovies"));
        }

        private void FixRatings(IDbConnection conn, IDbTransaction tran, string table)
        {
            var rows = conn.Query<Movie205>($"SELECT Id, Ratings FROM {table}");

            var corrected = new List<Movie206>();

            foreach (var row in rows)
            {
                var oldRatings = JsonSerializer.Deserialize<Ratings205>(row.Ratings, _serializerSettings);

                var newRatings = new Ratings206
                {
                    Tmdb = new RatingChild206
                    {
                        Votes = oldRatings.Votes,
                        Value = oldRatings.Value,
                        Type = RatingType206.User
                    }
                };

                corrected.Add(new Movie206
                {
                    Id = row.Id,
                    Ratings = JsonSerializer.Serialize(newRatings, _serializerSettings)
                });
            }

            var updateSql = $"UPDATE {table} SET Ratings = @Ratings WHERE Id = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private class Movie205
        {
            public int Id { get; set; }
            public string Ratings { get; set; }
        }

        private class Ratings205
        {
            public int Votes { get; set; }
            public decimal Value { get; set; }
        }

        private class Movie206
        {
            public int Id { get; set; }
            public string Ratings { get; set; }
        }

        private class Ratings206
        {
            public RatingChild206 Tmdb { get; set; }
            public RatingChild206 Imdb { get; set; }
            public RatingChild206 Metacritic { get; set; }
            public RatingChild206 RottenTomatoes { get; set; }
        }

        private class RatingChild206
        {
            public int Votes { get; set; }
            public decimal Value { get; set; }
            public RatingType206 Type { get; set; }
        }

        private enum RatingType206
        {
            User
        }
    }
}
