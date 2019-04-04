using NzbDrone.Core.Music;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using System.IO;
using System;
using NzbDrone.Common.Extensions;
using NzbDrone.Common;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalAlbumRelease
    {
        public LocalAlbumRelease()
        {
            LocalTracks = new List<LocalTrack>();

            // A dummy distance, will be replaced
            Distance = new Distance();
            Distance.Add("album_id", 1.0);
        }

        public LocalAlbumRelease(List<LocalTrack> tracks)
        {
            LocalTracks = tracks;

            // A dummy distance, will be replaced
            Distance = new Distance();
            Distance.Add("album_id", 1.0);
        }

        public List<LocalTrack> LocalTracks { get; set; }
        public int TrackCount => LocalTracks.Count;

        public TrackMapping TrackMapping { get; set; }
        public Distance Distance { get; set; }
        public AlbumRelease AlbumRelease { get; set; }
        public List<LocalTrack> ExistingTracks { get; set; }
        public bool NewDownload { get; set; }

        public void PopulateMatch()
        {
            if (AlbumRelease != null)
            {
                LocalTracks = LocalTracks.Concat(ExistingTracks).DistinctBy(x => x.Path).ToList();
                foreach (var localTrack in LocalTracks)
                {
                    localTrack.Release = AlbumRelease;
                    localTrack.Album = AlbumRelease.Album.Value;
                    localTrack.Artist = localTrack.Album.Artist.Value;
                    
                    if (TrackMapping.Mapping.ContainsKey(localTrack))
                    {
                        var track = TrackMapping.Mapping[localTrack].Item1;
                        localTrack.Tracks = new List<Track> { track };
                        localTrack.Distance = TrackMapping.Mapping[localTrack].Item2;
                    }
                }
            }
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", LocalTracks.Select(x => Path.GetDirectoryName(x.Path)).Distinct()) + "]";
        }
    }

    public class TrackMapping
    {
        public TrackMapping()
        {
            Mapping = new Dictionary<LocalTrack, Tuple<Track, Distance>>();
        }
        
        public Dictionary<LocalTrack, Tuple<Track, Distance>> Mapping { get; set; }
        public List<LocalTrack> LocalExtra { get; set; }
        public List<Track> MBExtra { get; set; }
    }
}
