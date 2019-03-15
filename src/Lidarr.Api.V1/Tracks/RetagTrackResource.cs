using System.Collections.Generic;
using System.Linq;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.Tracks
{
    public class TagDifference
    {
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
    
    public class RetagTrackResource : RestResource
    {
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }
        public List<int> TrackNumbers { get; set; }
        public int TrackFileId { get; set; }
        public string RelativePath { get; set; }
        public List<TagDifference> Changes { get; set; }
    }

    public static class RetagTrackResourceMapper
    {
        public static RetagTrackResource ToResource(this NzbDrone.Core.MediaFiles.RetagTrackFilePreview model)
        {
            if (model == null)
            {
                return null;
            }

            return new RetagTrackResource
            {
                ArtistId = model.ArtistId,
                AlbumId = model.AlbumId,
                TrackNumbers = model.TrackNumbers.ToList(),
                TrackFileId = model.TrackFileId,
                RelativePath = model.RelativePath,
                Changes = model.Changes.Select(x => new TagDifference {
                        Field = x.Key,
                        OldValue = x.Value.Item1,
                        NewValue = x.Value.Item2
                    }).ToList()
            };
        }

        public static List<RetagTrackResource> ToResource(this IEnumerable<NzbDrone.Core.MediaFiles.RetagTrackFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
