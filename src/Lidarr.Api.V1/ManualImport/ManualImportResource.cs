using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles.TrackImport.Manual;
using NzbDrone.Core.Qualities;
using Lidarr.Api.V1.Artist;
using Lidarr.Api.V1.Albums;
using Lidarr.Api.V1.Tracks;
using Lidarr.Http.REST;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace Lidarr.Api.V1.ManualImport
{
    public class ManualImportResource : RestResource
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public ArtistResource Artist { get; set; }
        public AlbumResource Album { get; set; }
        public int AlbumReleaseId { get; set; }
        public List<TrackResource> Tracks { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
        public ParsedTrackInfo AudioTags { get; set; }
        public bool AdditionalFile { get; set; }
        public bool ReplaceExistingFiles { get; set; }
        public bool DisableReleaseSwitching { get; set; }
    }

    public static class ManualImportResourceMapper
    {
        public static ManualImportResource ToResource(this ManualImportItem model)
        {
            if (model == null) return null;

            return new ManualImportResource
            {
                Id = model.Id,
                Path = model.Path,
                RelativePath = model.RelativePath,
                Name = model.Name,
                Size = model.Size,
                Artist = model.Artist.ToResource(),
                Album = model.Album.ToResource(),
                AlbumReleaseId = model.Release?.Id ?? 0,
                Tracks = model.Tracks.ToResource(),
                Quality = model.Quality,
                //QualityWeight
                DownloadId = model.DownloadId,
                Rejections = model.Rejections,
                AudioTags = model.Tags,
                AdditionalFile = model.AdditionalFile,
                ReplaceExistingFiles = model.ReplaceExistingFiles,
                DisableReleaseSwitching = model.DisableReleaseSwitching
            };
        }

        public static List<ManualImportResource> ToResource(this IEnumerable<ManualImportItem> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
