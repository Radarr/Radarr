﻿using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport.TMDb.Person
{
    public class TMDbPersonParser : TMDbParser
    {
        private readonly TMDbPersonSettings _settings;
        private readonly ISearchForNewMovie _skyhookProxy;

        public TMDbPersonParser(TMDbPersonSettings settings, ISearchForNewMovie skyhookProxy)
            : base(skyhookProxy)
        {
            _settings = settings;
            _skyhookProxy = skyhookProxy;
        }

        public override IList<Movie> ParseResponse(NetImportResponse importResponse)
        {
            var movies = new List<Movie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<PersonCreditsRoot>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            var crewTypes = GetCrewDepartments();

            if (_settings.PersonCast)
            {
                foreach (var movie in jsonResponse.cast)
                {
                    // Movies with no Year Fix
                    if (string.IsNullOrWhiteSpace(movie.release_date))
                    {
                        continue;
                    }

                    movies.AddIfNotNull(_skyhookProxy.MapMovie(movie));
                }
            }

            if (crewTypes.Count > 0)
            {
                foreach (var movie in jsonResponse.crew)
                {
                    // Movies with no Year Fix
                    if (string.IsNullOrWhiteSpace(movie.release_date))
                    {
                        continue;
                    }

                    if (crewTypes.Contains(movie.department))
                    {
                        movies.AddIfNotNull(_skyhookProxy.MapMovie(movie));
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
