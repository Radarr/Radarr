using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.TMDb.Person
{
    public class TMDbPersonParser : TMDbParser
    {
        private readonly TMDbPersonSettings _settings;

        public TMDbPersonParser(TMDbPersonSettings settings)
        {
            _settings = settings;
        }

        public override IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            var movies = new List<ImportListMovie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<PersonCreditsResource>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            var crewTypes = GetCrewDepartments();

            if (_settings.PersonCast)
            {
                var castMovies = FilterResults(jsonResponse.Cast);

                foreach (var movie in castMovies)
                {
                    // Movies with no Year Fix
                    if (string.IsNullOrWhiteSpace(movie.ReleaseDate))
                    {
                        continue;
                    }

                    movies.AddIfNotNull(new ImportListMovie { TmdbId = movie.Id });
                }
            }

            if (crewTypes.Count > 0)
            {
                var crewMovies = FilterResults(jsonResponse.Crew);

                foreach (var movie in crewMovies)
                {
                    // Movies with no Year Fix
                    if (string.IsNullOrWhiteSpace(movie.ReleaseDate))
                    {
                        continue;
                    }

                    if (crewTypes.Contains(movie.Department))
                    {
                        movies.AddIfNotNull(new ImportListMovie { TmdbId = movie.Id });
                    }
                }
            }

            return movies;
        }

        private IEnumerable<CreditsResultResource> FilterResults(IReadOnlyCollection<CreditsResultResource> results)
        {
            var items = results.ToList();

            if (_settings.MinVoteAverage.HasValue)
            {
                items = items.Where(r => _settings.MinVoteAverage.Value <= r.VoteAverage).ToList();
            }

            if (_settings.MinVotes.HasValue)
            {
                items = items.Where(r => _settings.MinVotes.Value <= r.VoteCount).ToList();
            }

            if (_settings.GenreIds.Any())
            {
                items = items.Where(r => r.GenreIds is { Count: > 0 } && _settings.GenreIds.Intersect(r.GenreIds).Any()).ToList();
            }

            if (_settings.LanguageCodes.Any())
            {
                items = items.Where(r => r.OriginalLanguage.IsNotNullOrWhiteSpace() && _settings.LanguageCodes.Contains(r.OriginalLanguage)).ToList();
            }

            return items;
        }

        private HashSet<string> GetCrewDepartments()
        {
            var creditsDepartment = new HashSet<string>();

            if (_settings.PersonCastDirector)
            {
                creditsDepartment.Add("Directing");
            }

            if (_settings.PersonCastProducer)
            {
                creditsDepartment.Add("Production");
            }

            if (_settings.PersonCastSound)
            {
                creditsDepartment.Add("Sound");
            }

            if (_settings.PersonCastWriting)
            {
                creditsDepartment.Add("Writing");
            }

            return creditsDepartment;
        }
    }
}
