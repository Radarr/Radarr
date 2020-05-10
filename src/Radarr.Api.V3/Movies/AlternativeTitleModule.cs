using System;
using System.Collections.Generic;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    public class AlternativeTitleModule : RadarrRestModule<AlternativeTitleResource>
    {
        private readonly IAlternativeTitleService _altTitleService;
        private readonly IMovieService _movieService;
        private readonly IEventAggregator _eventAggregator;

        public AlternativeTitleModule(IAlternativeTitleService altTitleService, IMovieService movieService, IEventAggregator eventAggregator)
            : base("/alttitle")
        {
            _altTitleService = altTitleService;
            _movieService = movieService;
            _eventAggregator = eventAggregator;

            GetResourceById = GetAltTitle;
            GetResourceAll = GetAltTitles;
        }

        private AlternativeTitleResource GetAltTitle(int id)
        {
            return _altTitleService.GetById(id).ToResource();
        }

        private List<AlternativeTitleResource> GetAltTitles()
        {
            var movieIdQuery = Request.Query.MovieId;

            if (movieIdQuery.HasValue)
            {
                int movieId = Convert.ToInt32(movieIdQuery.Value);

                return _altTitleService.GetAllTitlesForMovie(movieId).ToResource();
            }

            return _altTitleService.GetAllTitles().ToResource();
        }
    }
}
