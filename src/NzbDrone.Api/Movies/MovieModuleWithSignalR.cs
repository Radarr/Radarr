using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.SignalR;
using Radarr.Http;

namespace NzbDrone.Api.Movies
{
    public abstract class MovieModuleWithSignalR : RadarrRestModuleWithSignalR<MovieResource, Core.Movies.Movie>,
        IHandle<MovieGrabbedEvent>,
        IHandle<MovieDownloadedEvent>
    {
        protected readonly IMovieService _movieService;
        protected readonly IUpgradableSpecification _qualityUpgradableSpecification;

        protected MovieModuleWithSignalR(IMovieService movieService,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _movieService = movieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetMovie;
        }

        protected MovieModuleWithSignalR(IMovieService movieService,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _movieService = movieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetMovie;
        }

        protected MovieResource GetMovie(int id)
        {
            var movie = _movieService.GetMovie(id);
            return MapToResource(movie);
        }

        protected MovieResource MapToResource(Movie movie)
        {
            var resource = movie.ToResource();

            return resource;
        }

        public void Handle(MovieGrabbedEvent message)
        {
            var resource = message.Movie.Movie.ToResource();

            //add a grabbed field in MovieResource?
            //resource.Grabbed = true;
            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        public void Handle(MovieDownloadedEvent message)
        {
            var resource = message.Movie.Movie.ToResource();
            BroadcastResourceChange(ModelAction.Updated, resource);
        }
    }
}
