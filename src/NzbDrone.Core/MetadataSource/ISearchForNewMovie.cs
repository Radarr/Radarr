﻿using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        List<Movie> SearchForNewMovie(string title);

        Movie MapMovieToTmdbMovie(Movie movie);

        Movie MapMovie(SkyHook.Resource.MovieResult result);
    }
}
