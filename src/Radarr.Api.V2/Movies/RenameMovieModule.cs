using Radarr.Http.REST;
using NzbDrone.Core.MediaFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Radarr.Http;

namespace Radarr.Api.V2.Movies
{
    public class RenameMovieModule : RadarrRestModule<RenameMovieResource>
    {
        private readonly IRenameMovieFileService _renameMovieFileService;

        public RenameMovieModule(IRenameMovieFileService renameMovieFileService)
            : base("renameMovie")
        {
            _renameMovieFileService = renameMovieFileService;

            GetResourceAll = GetMovies; //TODO: GetResourceSingle?
        }

        private List<RenameMovieResource> GetMovies()
        {
            if(!Request.Query.MovieId.HasValue)
            {
                throw new BadRequestException("movieId is missing");
            }

            var movieId = (int)Request.Query.MovieId;

            return _renameMovieFileService.GetRenamePreviews(movieId).ToResource();
        }

    }
}
