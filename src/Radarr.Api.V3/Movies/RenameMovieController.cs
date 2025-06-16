using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaFiles;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("rename")]
    public class RenameMovieController : Controller
    {
        private readonly IRenameMovieFileService _renameMovieFileService;

        public RenameMovieController(IRenameMovieFileService renameMovieFileService)
        {
            _renameMovieFileService = renameMovieFileService;
        }

        [HttpGet]
        public List<RenameMovieResource> GetMovies([FromQuery(Name = "movieId")] List<int> movieIds)
        {
            if (movieIds is not { Count: not 0 })
            {
                throw new BadRequestException("movieId must be provided");
            }

            return _renameMovieFileService.GetRenamePreviews(movieIds).ToResource();
        }
    }
}
