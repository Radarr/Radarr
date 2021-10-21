using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaFiles;
using Radarr.Http;

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
        public List<RenameMovieResource> GetMovies(int movieId)
        {
            return _renameMovieFileService.GetRenamePreviews(movieId).ToResource();
        }
    }
}
