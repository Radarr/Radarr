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
    public interface IMediaFileRepository : IBasicRepository<BookFile>
    {
        List<BookFile> GetFilesByArtist(int authorId);
        List<BookFile> GetFilesByAlbum(int bookId);
        List<BookFile> GetUnmappedFiles();
        List<BookFile> GetFilesWithBasePath(string path);
        List<BookFile> GetFileWithPath(List<string> paths);
        BookFile GetFileWithPath(string path);
        void DeleteFilesByAlbum(int bookId);
        void UnlinkFilesByAlbum(int bookId);
    }

    public class MediaFileRepository : BasicRepository<BookFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        // always join with all the other good stuff
        // needed more often than not so better to load it all now
        protected override SqlBuilder Builder() => new SqlBuilder()
            .LeftJoin<BookFile, Book>((t, a) => t.BookId == a.Id)
            .LeftJoin<Book, Author>((album, artist) => album.AuthorMetadataId == artist.AuthorMetadataId)
            .LeftJoin<Author, AuthorMetadata>((a, m) => a.AuthorMetadataId == m.Id);

        protected override List<BookFile> Query(SqlBuilder builder) => Query(_database, builder).ToList();

        public static IEnumerable<BookFile> Query(IDatabase database, SqlBuilder builder)
        {
            return database.QueryJoined<BookFile, Book, Author, AuthorMetadata>(builder, (file, album, artist, metadata) => Map(file, album, artist, metadata));
        }

        private static BookFile Map(BookFile file, Book album, Author artist, AuthorMetadata metadata)
        {
            file.Album = album;

            if (artist != null)
            {
                artist.Metadata = metadata;
            }

            file.Artist = artist;

            return file;
        }

        public List<BookFile> GetFilesByArtist(int authorId)
        {
            return Query(Builder().Where<Author>(a => a.Id == authorId));
        }

        public List<BookFile> GetFilesByAlbum(int bookId)
        {
            return Query(Builder().Where<BookFile>(f => f.BookId == bookId));
        }

        public List<BookFile> GetUnmappedFiles()
        {
            //x.Id == null is converted to SQL, so warning incorrect
#pragma warning disable CS0472
            return _database.Query<BookFile>(new SqlBuilder().Select(typeof(BookFile))
                                              .LeftJoin<BookFile, Book>((f, t) => f.BookId == t.Id)
                                              .Where<Book>(t => t.Id == null)).ToList();
#pragma warning restore CS0472
        }

        public void DeleteFilesByAlbum(int bookId)
        {
            Delete(x => x.BookId == bookId);
        }

        public void UnlinkFilesByAlbum(int bookId)
        {
            var files = Query(x => x.BookId == bookId);
            files.ForEach(x => x.BookId = 0);
            SetFields(files, f => f.BookId);
        }

        public List<BookFile> GetFilesWithBasePath(string path)
        {
            // ensure path ends with a single trailing path separator to avoid matching partial paths
            var safePath = path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return _database.Query<BookFile>(new SqlBuilder().Where<BookFile>(x => x.Path.StartsWith(safePath))).ToList();
        }

        public BookFile GetFileWithPath(string path)
        {
            return Query(x => x.Path == path).SingleOrDefault();
        }

        public List<BookFile> GetFileWithPath(List<string> paths)
        {
            // use more limited join for speed
            var builder = new SqlBuilder()
                .LeftJoin<BookFile, Book>((f, t) => f.BookId == t.Id);

            var all = _database.QueryJoined<BookFile, Book>(builder, (file, book) => MapTrack(file, book)).ToList();

            var joined = all.Join(paths, x => x.Path, x => x, (file, path) => file, PathEqualityComparer.Instance).ToList();
            return joined;
        }

        private BookFile MapTrack(BookFile file, Book book)
        {
            file.Album = book;
            return file;
        }
    }
}
