using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class MediaFileRepositoryFixture : DbTest<MediaFileRepository, BookFile>
    {
        private Author _artist;
        private Book _album;

        [SetUp]
        public void Setup()
        {
            var meta = Builder<AuthorMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(meta);

            _artist = Builder<Author>.CreateNew()
                .With(a => a.AuthorMetadataId = meta.Id)
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(_artist);

            _album = Builder<Book>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.AuthorMetadataId = _artist.AuthorMetadataId)
                .Build();
            Db.Insert(_album);

            var files = Builder<BookFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel(Quality.MP3_320))
                .TheFirst(5)
                .With(c => c.BookId = _album.Id)
                .TheFirst(1)
                .With(c => c.Path = @"C:\Test\Path\Artist\somefile1.flac".AsOsAgnostic())
                .TheNext(1)
                .With(c => c.Path = @"C:\Test\Path\Artist\somefile2.flac".AsOsAgnostic())
                .BuildListOfNew();
            Db.InsertMany(files);
        }

        [Test]
        public void get_files_by_artist()
        {
            VerifyData();
            var artistFiles = Subject.GetFilesByArtist(_artist.Id);
            VerifyEagerLoaded(artistFiles);

            artistFiles.Should().OnlyContain(c => c.Artist.Value.Id == _artist.Id);
        }

        [Test]
        public void get_unmapped_files()
        {
            VerifyData();
            var unmappedfiles = Subject.GetUnmappedFiles();
            VerifyUnmapped(unmappedfiles);

            unmappedfiles.Should().HaveCount(5);
        }

        [TestCase("C:\\Test\\Path")]
        [TestCase("C:\\Test\\Path\\")]
        public void get_files_by_base_path_should_cope_with_trailing_slash(string dir)
        {
            VerifyData();
            var firstReleaseFiles = Subject.GetFilesWithBasePath(dir.AsOsAgnostic());

            firstReleaseFiles.Should().HaveCount(2);
        }

        [TestCase("C:\\Test\\Path")]
        [TestCase("C:\\Test\\Path\\")]
        public void get_files_by_base_path_should_not_get_files_for_partial_path(string dir)
        {
            VerifyData();

            var files = Builder<BookFile>.CreateListOfSize(2)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel(Quality.MP3_320))
                .TheFirst(1)
                .With(c => c.Path = @"C:\Test\Path2\Artist\somefile1.flac".AsOsAgnostic())
                .TheNext(1)
                .With(c => c.Path = @"C:\Test\Path2\Artist\somefile2.flac".AsOsAgnostic())
                .BuildListOfNew();
            Db.InsertMany(files);

            var firstReleaseFiles = Subject.GetFilesWithBasePath(dir.AsOsAgnostic());
            firstReleaseFiles.Should().HaveCount(2);
        }

        [Test]
        public void get_file_by_path()
        {
            VerifyData();
            var file = Subject.GetFileWithPath(@"C:\Test\Path\Artist\somefile2.flac".AsOsAgnostic());

            file.Should().NotBeNull();
            file.Album.IsLoaded.Should().BeTrue();
            file.Album.Value.Should().NotBeNull();
            file.Artist.IsLoaded.Should().BeTrue();
            file.Artist.Value.Should().NotBeNull();
        }

        [Test]
        public void get_files_by_album()
        {
            VerifyData();
            var files = Subject.GetFilesByAlbum(_album.Id);
            VerifyEagerLoaded(files);

            files.Should().OnlyContain(c => c.BookId == _album.Id);
        }

        private void VerifyData()
        {
            Db.All<Author>().Should().HaveCount(1);
            Db.All<Book>().Should().HaveCount(1);
            Db.All<BookFile>().Should().HaveCount(10);
        }

        private void VerifyEagerLoaded(List<BookFile> files)
        {
            foreach (var file in files)
            {
                file.Album.IsLoaded.Should().BeTrue();
                file.Album.Value.Should().NotBeNull();
                file.Artist.IsLoaded.Should().BeTrue();
                file.Artist.Value.Should().NotBeNull();
                file.Artist.Value.Metadata.IsLoaded.Should().BeTrue();
                file.Artist.Value.Metadata.Value.Should().NotBeNull();
            }
        }

        private void VerifyUnmapped(List<BookFile> files)
        {
            foreach (var file in files)
            {
                file.Album.IsLoaded.Should().BeFalse();
                file.Album.Value.Should().BeNull();
                file.Artist.IsLoaded.Should().BeFalse();
                file.Artist.Value.Should().BeNull();
            }
        }

        [Test]
        public void delete_files_by_album_should_work_if_join_fails()
        {
            Db.Delete(_album);
            Subject.DeleteFilesByAlbum(_album.Id);

            Db.All<BookFile>().Where(x => x.BookId == _album.Id).Should().HaveCount(0);
        }
    }
}
