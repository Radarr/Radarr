using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using Radarr.Http;

namespace Radarr.Api.V3.Movies;

[V3ApiController("movie")]
public class MovieFolderController : Controller
{
    private readonly IMovieService _movieService;
    private readonly IBuildFileNames _fileNameBuilder;

    public MovieFolderController(IMovieService movieService, IBuildFileNames fileNameBuilder)
    {
        _movieService = movieService;
        _fileNameBuilder = fileNameBuilder;
    }

    [HttpGet("{id:int}/folder")]
    [Produces("application/json")]
    public Ok<MovieFolderResource> GetFolder([FromRoute] int id)
    {
        var series = _movieService.GetMovie(id);
        var folder = _fileNameBuilder.GetMovieFolder(series);

        return TypedResults.Ok(new MovieFolderResource
        {
            Folder = folder
        });
    }
}
