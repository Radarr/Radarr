using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(186)]
    public class fix_tmdb_duplicates : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(FixMovies);
            Delete.Index("IX_Movies_TmdbId").OnTable("Movies");
            Alter.Table("Movies").AlterColumn("TmdbId").AsInt32().Unique();
        }

        private void FixMovies(IDbConnection conn, IDbTransaction tran)
        {
            var movieRows = conn.Query<MovieEntity185>($"SELECT Id, TmdbId, Added, LastInfoSync, MovieFileId FROM Movies");

            // Only process if there are movies existing in the DB
            if (movieRows.Any())
            {
                var movieGroups = movieRows.GroupBy(m => m.TmdbId);
                var problemMovies = movieGroups.Where(g => g.Count() > 1);
                var purgeMovies = new List<MovieEntity185>();

                // Don't do anything if there are no duplicate movies
                if (!problemMovies.Any())
                {
                    return;
                }

                //Process duplicates to pick which to purge
                foreach (var problemGroup in problemMovies)
                {
                    var moviesWithFiles = problemGroup.Where(m => m.MovieFileId > 0);
                    var moviesWithInfo = problemGroup.Where(m => m.LastInfoSync != null);

                    // If we only have one with file keep it
                    if (moviesWithFiles.Count() == 1)
                    {
                        purgeMovies.AddRange(problemGroup.Where(m => m.MovieFileId == 0).Select(m => m));
                        continue;
                    }

                    // If we only have one with info keep it
                    if (moviesWithInfo.Count() == 1)
                    {
                        purgeMovies.AddRange(problemGroup.Where(m => m.LastInfoSync == null).Select(m => m));
                        continue;
                    }

                    // Else Prioritize by having file then Added
                    purgeMovies.AddRange(problemGroup.OrderByDescending(m => m.MovieFileId > 0 ? 1 : 0).ThenBy(m => m.Added).Skip(1).Select(m => m));
                }

                if (purgeMovies.Count > 0)
                {
                    var deleteSql = "DELETE FROM Movies WHERE Id = @Id";
                    conn.Execute(deleteSql, purgeMovies, transaction: tran);
                }

                // Delete duplicates, files, metadata, history, etc...
                // (Or just the movie and let housekeeper take the rest)
            }
        }

        private class MovieEntity185
        {
            public int Id { get; set; }
            public int TmdbId { get; set; }
            public DateTime Added { get; set; }
            public DateTime? LastInfoSync { get; set; }
            public int MovieFileId { get; set; }
        }
    }
}
