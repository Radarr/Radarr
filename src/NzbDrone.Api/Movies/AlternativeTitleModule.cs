using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource.RadarrAPI;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using Radarr.Http;

namespace NzbDrone.Api.Movies
{
    public class AlternativeTitleModule : RadarrRestModule<AlternativeTitleResource>
    {
        private readonly IAlternativeTitleService _altTitleService;
        private readonly IMovieService _movieService;
        private readonly IRadarrAPIClient _radarrApi;
        private readonly IEventAggregator _eventAggregator;

        public AlternativeTitleModule(IAlternativeTitleService altTitleService, IMovieService movieService, IRadarrAPIClient radarrApi, IEventAggregator eventAggregator)
            : base("/alttitle")
        {
            _altTitleService = altTitleService;
            _movieService = movieService;
            _radarrApi = radarrApi;
            CreateResource = AddTitle;
            GetResourceById = GetTitle;
            _eventAggregator = eventAggregator;
        }

        private int AddTitle(AlternativeTitleResource altTitle)
        {
            var title = altTitle.ToModel();
            var movie = _movieService.GetMovie(altTitle.MovieId);
            var newTitle = _radarrApi.AddNewAlternativeTitle(title, movie.TmdbId);

            var addedTitle = _altTitleService.AddAltTitle(newTitle, movie);
            _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));
            return addedTitle.Id;
        }

        private AlternativeTitleResource GetTitle(int id)
        {
            return _altTitleService.GetById(id).ToResource();
        }
    }
}
