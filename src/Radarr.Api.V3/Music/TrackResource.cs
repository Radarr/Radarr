using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;
using Radarr.Api.V3.MediaItems;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Music
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S6968", Justification = "Follows existing resource patterns")]
    public class TrackResource : RestResource, IMediaResource
    {
        public TrackResource()
        {
            Monitored = true;
        }

        public int? AlbumId { get; set; }
        public string Title { get; set; }
        public string ForeignTrackId { get; set; }
        public int TrackNumber { get; set; }
        public int DiscNumber { get; set; }
        public int? DurationSeconds { get; set; }

        public bool Monitored { get; set; }
        public bool EffectivelyMonitored { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public bool? HasFile { get; set; }
        public long? SizeOnDisk { get; set; }
    }

    public static class TrackResourceMapper
    {
        public static TrackResource ToResource(this Track model)
        {
            if (model == null)
            {
                return null;
            }

            return new TrackResource
            {
                Id = model.Id,
                AlbumId = model.AlbumId,
                Title = model.Title,
                ForeignTrackId = model.ForeignTrackId,
                TrackNumber = model.TrackNumber,
                DiscNumber = model.DiscNumber,
                DurationSeconds = model.DurationSeconds,
                Monitored = model.Monitored,
                QualityProfileId = model.QualityProfileId,
                Path = model.Path,
                RootFolderPath = model.RootFolderPath,
                Added = model.Added,
                Tags = model.Tags,
                LastSearchTime = model.LastSearchTime
            };
        }

        public static Track ToModel(this TrackResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Track
            {
                Id = resource.Id,
                AlbumId = resource.AlbumId,
                Title = resource.Title,
                ForeignTrackId = resource.ForeignTrackId,
                TrackNumber = resource.TrackNumber,
                DiscNumber = resource.DiscNumber,
                DurationSeconds = resource.DurationSeconds,
                Monitored = resource.Monitored,
                QualityProfileId = resource.QualityProfileId,
                Path = resource.Path,
                RootFolderPath = resource.RootFolderPath,
                Tags = resource.Tags ?? new HashSet<int>()
            };
        }

        public static Track ToModel(this TrackResource resource, Track track)
        {
            var updatedTrack = resource.ToModel();

            track.AlbumId = updatedTrack.AlbumId;
            track.Title = updatedTrack.Title;
            track.ForeignTrackId = updatedTrack.ForeignTrackId;
            track.TrackNumber = updatedTrack.TrackNumber;
            track.DiscNumber = updatedTrack.DiscNumber;
            track.DurationSeconds = updatedTrack.DurationSeconds;
            track.Monitored = updatedTrack.Monitored;
            track.QualityProfileId = updatedTrack.QualityProfileId;
            track.Path = updatedTrack.Path;
            track.RootFolderPath = updatedTrack.RootFolderPath;
            track.Tags = updatedTrack.Tags;

            return track;
        }

        public static List<TrackResource> ToResource(this IEnumerable<Track> tracks)
        {
            return tracks.Select(ToResource).ToList();
        }

        public static List<Track> ToModel(this IEnumerable<TrackResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
