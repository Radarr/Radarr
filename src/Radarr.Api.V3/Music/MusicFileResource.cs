using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Music
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S6968", Justification = "Follows existing resource patterns")]
    public class MusicFileResource : RestResource
    {
        public int? TrackId { get; set; }
        public int? AlbumId { get; set; }
        public string RelativePath { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public string AudioFormat { get; set; }
        public int? Bitrate { get; set; }
        public int? SampleRate { get; set; }
        public int? Channels { get; set; }
    }

    public static class MusicFileResourceMapper
    {
        public static MusicFileResource ToResource(this MusicFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new MusicFileResource
            {
                Id = model.Id,
                TrackId = model.TrackId,
                AlbumId = model.AlbumId,
                RelativePath = model.RelativePath,
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                ReleaseGroup = model.ReleaseGroup,
                Quality = model.Quality,
                AudioFormat = model.AudioFormat,
                Bitrate = model.Bitrate,
                SampleRate = model.SampleRate,
                Channels = model.Channels
            };
        }

        public static List<MusicFileResource> ToResource(this IEnumerable<MusicFile> musicFiles)
        {
            return musicFiles.Select(ToResource).ToList();
        }
    }
}
