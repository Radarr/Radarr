using System;
using System.IO;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using Radarr.Http.REST;

namespace Radarr.Api.V2.MovieFiles
{
    public class MovieFileResource : RestResource
    {
        public int MovieId { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoResource MediaInfo { get; set; }

        public bool QualityCutoffNotMet { get; set; }
    }

    public static class MovieFileResourceMapper
    {
        private static MovieFileResource ToResource(this MovieFile model)
        {
            if (model == null) return null;

            return new MovieFileResource
            {
                Id = model.Id,

                MovieId = model.MovieId,
                RelativePath = model.RelativePath,
                //Path
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
                //QualityCutoffNotMet
            };

        }

        public static MovieFileResource ToResource(this MovieFile model, NzbDrone.Core.Movies.Movie movie)
        {
            if (model == null) return null;

            return new MovieFileResource
            {
                Id = model.Id,

                MovieId = model.MovieId,
                RelativePath = model.RelativePath,
                Path = Path.Combine(movie.Path, model.RelativePath),
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
                // QualityCutoffNotMet = upgradableSpecification.CutoffNotMet(movie.Profile.Value, model.Quality)
            };
        }

        public static MovieFileResource ToResource(this MovieFile model, NzbDrone.Core.Movies.Movie movie, IQualityUpgradableSpecification upgradableSpecification)
        {
            if (model == null) return null;

            return new MovieFileResource
            {
                Id = model.Id,

                MovieId = model.MovieId,
                RelativePath = model.RelativePath,
                Path = Path.Combine(movie.Path, model.RelativePath),
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
                QualityCutoffNotMet = upgradableSpecification.CutoffNotMet(movie.Profile.Value, model.Quality)
            };
        }
    }
}
