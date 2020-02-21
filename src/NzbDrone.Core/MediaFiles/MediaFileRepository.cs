using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<TrackFile>
    {
        List<TrackFile> GetFilesByArtist(int artistId);
        List<TrackFile> GetFilesByAlbum(int albumId);
        List<TrackFile> GetFilesByRelease(int releaseId);
        List<TrackFile> GetUnmappedFiles();
        List<TrackFile> GetFilesWithBasePath(string path);
        List<TrackFile> GetFileWithPath(List<string> paths);
        TrackFile GetFileWithPath(string path);
        void DeleteFilesByAlbum(int albumId);
        void UnlinkFilesByAlbum(int albumId);
    }

    public class MediaFileRepository : BasicRepository<TrackFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        // always join with all the other good stuff
        // needed more often than not so better to load it all now
        protected override SqlBuilder Builder() => new SqlBuilder()
            .LeftJoin<TrackFile, Track>((t, x) => t.Id == x.TrackFileId)
            .LeftJoin<TrackFile, Album>((t, a) => t.AlbumId == a.Id)
            .LeftJoin<Album, Artist>((album, artist) => album.ArtistMetadataId == artist.ArtistMetadataId)
            .LeftJoin<Artist, ArtistMetadata>((a, m) => a.ArtistMetadataId == m.Id);

        protected override List<TrackFile> Query(SqlBuilder builder) => Query(_database, builder).ToList();

        public static IEnumerable<TrackFile> Query(IDatabase database, SqlBuilder builder)
        {
            var fileDictionary = new Dictionary<int, TrackFile>();

            _ = database.QueryJoined<TrackFile, Track, Album, Artist, ArtistMetadata>(builder, (file, track, album, artist, metadata) => Map(fileDictionary, file, track, album, artist, metadata));

            return fileDictionary.Values;
        }

        private static TrackFile Map(Dictionary<int, TrackFile> dict, TrackFile file, Track track, Album album, Artist artist, ArtistMetadata metadata)
        {
            if (!dict.TryGetValue(file.Id, out var entry))
            {
                if (artist != null)
                {
                    artist.Metadata = metadata;
                }

                entry = file;
                entry.Tracks = new List<Track>();
                entry.Album = album;
                entry.Artist = artist;
                dict.Add(entry.Id, entry);
            }

            if (track != null)
            {
                entry.Tracks.Value.Add(track);
            }

            return entry;
        }

        public List<TrackFile> GetFilesByArtist(int artistId)
        {
            return Query(Builder().LeftJoin<Track, AlbumRelease>((t, r) => t.AlbumReleaseId == r.Id)
                         .Where<AlbumRelease>(r => r.Monitored == true)
                         .Where<Artist>(a => a.Id == artistId));
        }

        public List<TrackFile> GetFilesByAlbum(int albumId)
        {
            return Query(Builder().LeftJoin<Track, AlbumRelease>((t, r) => t.AlbumReleaseId == r.Id)
                         .Where<AlbumRelease>(r => r.Monitored == true)
                         .Where<TrackFile>(f => f.AlbumId == albumId));
        }

        public List<TrackFile> GetUnmappedFiles()
        {
            //x.Id == null is converted to SQL, so warning incorrect
#pragma warning disable CS0472
            return _database.Query<TrackFile>(new SqlBuilder().Select(typeof(TrackFile))
                                              .LeftJoin<TrackFile, Track>((f, t) => f.Id == t.TrackFileId)
                                              .Where<Track>(t => t.Id == null)).ToList();
#pragma warning restore CS0472
        }

        public void DeleteFilesByAlbum(int albumId)
        {
            Delete(x => x.AlbumId == albumId);
        }

        public void UnlinkFilesByAlbum(int albumId)
        {
            var files = Query(x => x.AlbumId == albumId);
            files.ForEach(x => x.AlbumId = 0);
            SetFields(files, f => f.AlbumId);
        }

        public List<TrackFile> GetFilesByRelease(int releaseId)
        {
            return Query(Builder().Where<Track>(x => x.AlbumReleaseId == releaseId));
        }

        public List<TrackFile> GetFilesWithBasePath(string path)
        {
            // ensure path ends with a single trailing path separator to avoid matching partial paths
            var safePath = path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return _database.Query<TrackFile>(new SqlBuilder().Where<TrackFile>(x => x.Path.StartsWith(safePath))).ToList();
        }

        public TrackFile GetFileWithPath(string path)
        {
            return Query(x => x.Path == path).SingleOrDefault();
        }

        public List<TrackFile> GetFileWithPath(List<string> paths)
        {
            // use more limited join for speed
            var builder = new SqlBuilder()
                .LeftJoin<TrackFile, Track>((f, t) => f.Id == t.TrackFileId);

            var dict = new Dictionary<int, TrackFile>();
            _ = _database.QueryJoined<TrackFile, Track>(builder, (file, track) => MapTrack(dict, file, track)).ToList();
            var all = dict.Values.ToList();

            var joined = all.Join(paths, x => x.Path, x => x, (file, path) => file, PathEqualityComparer.Instance).ToList();
            return joined;
        }

        private TrackFile MapTrack(Dictionary<int, TrackFile> dict, TrackFile file, Track track)
        {
            if (!dict.TryGetValue(file.Id, out var entry))
            {
                entry = file;
                entry.Tracks = new List<Track>();
                dict.Add(entry.Id, entry);
            }

            if (track != null)
            {
                entry.Tracks.Value.Add(track);
            }

            return entry;
        }
    }
}
