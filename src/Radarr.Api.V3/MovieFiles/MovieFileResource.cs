using System;
using System.Collections.Generic;
using System.IO;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using Radarr.Api.V3.CustomFormats;
using Radarr.Http.REST;

namespace Radarr.Api.V3.MovieFiles
{
    public class MovieFileResource : RestResource
    {
        public int MovieId { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public int IndexerFlags { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public MediaInfoResource MediaInfo { get; set; }
        public string OriginalFilePath { get; set; }
        public bool QualityCutoffNotMet { get; set; }
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public string Edition { get; set; }
    }

    public static class MovieFileResourceMapper
    {
        private static MovieFileResource ToResource(this MovieFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new MovieFileResource
            {
                Id = model.Id,

                MovieId = model.MovieId,
                RelativePath = model.RelativePath,

                //Path
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                IndexerFlags = (int)model.IndexerFlags,
                Quality = model.Quality,
                Languages = model.Languages,
                ReleaseGroup = model.ReleaseGroup,
                Edition = model.Edition,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
                OriginalFilePath = model.OriginalFilePath
            };
        }

        public static MovieFileResource ToResource(this MovieFile model, NzbDrone.Core.Movies.Movie movie, IUpgradableSpecification upgradableSpecification, ICustomFormatCalculationService formatCalculator)
        {
            if (model == null)
            {
                return null;
            }

            return new MovieFileResource
            {
                Id = model.Id,

                MovieId = model.MovieId,
                RelativePath = model.RelativePath,
                Path = Path.Combine(movie.Path, model.RelativePath),
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                IndexerFlags = (int)model.IndexerFlags,
                Quality = model.Quality,
                Languages = model.Languages,
                Edition = model.Edition,
                ReleaseGroup = model.ReleaseGroup,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
                QualityCutoffNotMet = upgradableSpecification?.QualityCutoffNotMet(movie.Profile, model.Quality) ?? false,
                OriginalFilePath = model.OriginalFilePath,
                CustomFormats = formatCalculator.ParseCustomFormat(model).ToResource()
            };
        }
    }
}
