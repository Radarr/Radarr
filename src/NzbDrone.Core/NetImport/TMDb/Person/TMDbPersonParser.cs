using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport.TMDb.Person
{
    public class TMDbPersonParser : TMDbParser
    {
        private readonly TMDbPersonSettings _settings;

        public TMDbPersonParser(TMDbPersonSettings settings)
        {
            _settings = settings;
        }

        public override IList<Movie> ParseResponse(NetImportResponse importResponse)
        {
            var movies = new List<Movie>();

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
                foreach (var movie in jsonResponse.Cast)
                {
                    // Movies with no Year Fix
                    if (string.IsNullOrWhiteSpace(movie.ReleaseDate))
                    {
                        continue;
                    }

                    movies.AddIfNotNull(new Movie { TmdbId = movie.Id });
                }
            }

            if (crewTypes.Count > 0)
            {
                foreach (var movie in jsonResponse.Crew)
                {
                    // Movies with no Year Fix
                    if (string.IsNullOrWhiteSpace(movie.ReleaseDate))
                    {
                        continue;
                    }

                    if (crewTypes.Contains(movie.Department))
                    {
                        movies.AddIfNotNull(new Movie { TmdbId = movie.Id });
                    }
                }
            }

            return movies;
        }

        private List<string> GetCrewDepartments()
        {
            var creditsDepartment = new List<string>();

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
