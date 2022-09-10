using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaFiles;
using Radarr.Http;

namespace Radarr.Api.V4.Movies
{
    [V4ApiController("rename")]
    public class RenameMovieController : Controller
    {
        private readonly IRenameMovieFileService _renameMovieFileService;

        public RenameMovieController(IRenameMovieFileService renameMovieFileService)
        {
            _renameMovieFileService = renameMovieFileService;
        }

        [HttpGet]
        public List<RenameMovieResource> GetMovies(int movieId)
        {
            return _renameMovieFileService.GetRenamePreviews(movieId).ToResource();
        }
    }
}
