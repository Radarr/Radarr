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
