using System;
using System.IO;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.TrackFiles
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
        public QualityModel Quality { get; set; }

        public bool QualityCutoffNotMet { get; set; }
    }

    public static class TrackFileResourceMapper
    {
        private static TrackFileResource ToResource(this Core.MediaFiles.TrackFile model)
        {
            if (model == null) return null;

            return new TrackFileResource
            {
                Id = model.Id,

                ArtistId = model.ArtistId,
                AlbumId = model.AlbumId,
                RelativePath = model.RelativePath,
                //Path
                Size = model.Size,
                DateAdded = model.DateAdded,
                //SceneName = model.SceneName,
                Quality = model.Quality,
                //QualityCutoffNotMet
            };
        }

        public static TrackFileResource ToResource(this Core.MediaFiles.TrackFile model, Core.Music.Artist artist, Core.DecisionEngine.IQualityUpgradableSpecification qualityUpgradableSpecification)
        {
            if (model == null) return null;

            return new TrackFileResource
            {
                Id = model.Id,

                ArtistId = model.ArtistId,
                AlbumId = model.AlbumId,
                RelativePath = model.RelativePath,
                Path = Path.Combine(artist.Path, model.RelativePath),
                Size = model.Size,
                DateAdded = model.DateAdded,
                //SceneName = model.SceneName,
                Quality = model.Quality,
                QualityCutoffNotMet = qualityUpgradableSpecification.CutoffNotMet(artist.Profile.Value, model.Quality)
            };
        }
    }
}
