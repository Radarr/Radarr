using NzbDrone.Common.Crypto;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles.TrackImport.Manual;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;
using Lidarr.Api.V1.Artist;
using Lidarr.Api.V1.Albums;
using Lidarr.Api.V1.Tracks;
using Lidarr.Http.REST;
using System.Collections.Generic;
using System.Linq;

namespace Lidarr.Api.V1.ManualImport
{
    public class ManualImportResource : RestResource
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public ArtistResource Artist { get; set; }
        public AlbumResource Album { get; set; }
        public List<TrackResource> Tracks { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public int QualityWeight { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
    }

    public static class ManualImportResourceMapper
    {
        public static ManualImportResource ToResource(this ManualImportItem model)
        {
            if (model == null) return null;

            return new ManualImportResource
            {
                Id = HashConverter.GetHashInt31(model.Path),
                Path = model.Path,
                RelativePath = model.RelativePath,
                FolderName = model.FolderName,
                Name = model.Name,
                Size = model.Size,
                Artist = model.Artist.ToResource(),
                Album = model.Album.ToResource(),
                Tracks = model.Tracks.ToResource(),
                Quality = model.Quality,
                Language = model.Language,
                //QualityWeight
                DownloadId = model.DownloadId,
                Rejections = model.Rejections
            };
        }

        public static List<ManualImportResource> ToResource(this IEnumerable<ManualImportItem> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
