using System;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using Lidarr.Http.REST;
using NzbDrone.Common.Extensions;

namespace Lidarr.Api.V1.TrackFiles
{
    public class TrackFileResource : RestResource
    {
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        //public string SceneName { get; set; }
        public Language Language { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoResource MediaInfo { get; set; }

        public bool QualityCutoffNotMet { get; set; }
        public bool LanguageCutoffNotMet { get; set; }

    }

    public static class TrackFileResourceMapper
    {
        private static TrackFileResource ToResource(this TrackFile model)
        {
            if (model == null) return null;

            return new TrackFileResource
            {
                Id = model.Id,

                ArtistId = model.Artist.Value.Id,
                AlbumId = model.AlbumId,
                RelativePath = model.Artist.Value.Path.GetRelativePath(model.Path),
                Path = model.Path,
                Size = model.Size,
                DateAdded = model.DateAdded,
               // SceneName = model.SceneName,
                Language = model.Language,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource()
                //QualityCutoffNotMet
            };

        }

        public static TrackFileResource ToResource(this TrackFile model, NzbDrone.Core.Music.Artist artist, IUpgradableSpecification upgradableSpecification)
        {
            if (model == null) return null;

            return new TrackFileResource
            {
                Id = model.Id,

                ArtistId = artist.Id,
                AlbumId = model.AlbumId,
                Path = model.Path,
                RelativePath = artist.Path.GetRelativePath(model.Path),
                Size = model.Size,
                DateAdded = model.DateAdded,
                //SceneName = model.SceneName,
                Language = model.Language,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(),
                QualityCutoffNotMet = upgradableSpecification.QualityCutoffNotMet(artist.QualityProfile.Value, model.Quality),
                LanguageCutoffNotMet = upgradableSpecification.LanguageCutoffNotMet(artist.LanguageProfile.Value, model.Language)
            };
        }
    }
}
