using Radarr.Http.REST;
using NzbDrone.Core.MediaFiles;
using System.Collections.Generic;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    public class RenameMovieModule : RadarrRestModule<RenameMovieResource>
    {
        private readonly IRenameMovieFileService _renameMovieFileService;

        public RenameMovieModule(IRenameMovieFileService renameMovieFileService)
            : base("rename")
        {
            _renameMovieFileService = renameMovieFileService;

            GetResourceAll = GetMovies;
        }

        private List<RenameMovieResource> GetMovies()
        {
            int movieId;

            if(Request.Query.MovieId.HasValue)
            {
                movieId = (int)Request.Query.MovieId;
            }
            else
            {
                throw new BadRequestException("movieId is missing");
            }

            return _renameMovieFileService.GetRenamePreviews(movieId).ToResource();
        }

    }
}
