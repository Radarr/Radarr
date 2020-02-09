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
    public class MediaFileRepositoryFixture : DbTest<MediaFileRepository, TrackFile>
    {
        private Artist _artist;
        private Album _album;
        private List<AlbumRelease> _releases;

        [SetUp]
        public void Setup()
        {
            var meta = Builder<ArtistMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(meta);

            _artist = Builder<Artist>.CreateNew()
                .With(a => a.ArtistMetadataId = meta.Id)
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(_artist);

            _album = Builder<Album>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.ArtistMetadataId = _artist.ArtistMetadataId)
                .Build();
            Db.Insert(_album);

            _releases = Builder<AlbumRelease>.CreateListOfSize(2)
                .All()
                .With(a => a.Id = 0)
                .With(a => a.AlbumId = _album.Id)
                .TheFirst(1)
                .With(a => a.Monitored = true)
                .TheNext(1)
                .With(a => a.Monitored = false)
                .Build().ToList();
            Db.InsertMany(_releases);

            var files = Builder<TrackFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel(Quality.MP3_192))
                .TheFirst(5)
                .With(c => c.AlbumId = _album.Id)
                .TheFirst(1)
                .With(c => c.Path = @"C:\Test\Path\Artist\somefile1.flac".AsOsAgnostic())
                .TheNext(1)
                .With(c => c.Path = @"C:\Test\Path\Artist\somefile2.flac".AsOsAgnostic())
                .BuildListOfNew();
            Db.InsertMany(files);

            var track = Builder<Track>.CreateListOfSize(10)
                .All()
                .With(a => a.Id = 0)
                .TheFirst(4)
                .With(a => a.AlbumReleaseId = _releases[0].Id)
                .TheFirst(1)
                .With(a => a.TrackFileId = files[0].Id)
                .TheNext(1)
                .With(a => a.TrackFileId = files[1].Id)
                .TheNext(1)
                .With(a => a.TrackFileId = files[2].Id)
                .TheNext(1)
                .With(a => a.TrackFileId = files[3].Id)
                .TheNext(1)
                .With(a => a.TrackFileId = files[4].Id)
                .With(a => a.AlbumReleaseId = _releases[1].Id)
                .TheNext(5)
                .With(a => a.TrackFileId = 0)
                .Build();
            Db.InsertMany(track);
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

        [Test]
        public void get_files_by_release()
        {
            VerifyData();
            var firstReleaseFiles = Subject.GetFilesByRelease(_releases[0].Id);
            var secondReleaseFiles = Subject.GetFilesByRelease(_releases[1].Id);
            VerifyEagerLoaded(firstReleaseFiles);
            VerifyEagerLoaded(secondReleaseFiles);

            firstReleaseFiles.Should().HaveCount(4);
            secondReleaseFiles.Should().HaveCount(1);
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

            var files = Builder<TrackFile>.CreateListOfSize(2)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel(Quality.MP3_192))
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
            file.Tracks.IsLoaded.Should().BeTrue();
            file.Tracks.Value.Should().NotBeNull();
            file.Tracks.Value.Should().NotBeEmpty();
            file.Album.IsLoaded.Should().BeTrue();
            file.Album.Value.Should().NotBeNull();
            file.Artist.IsLoaded.Should().BeTrue();
            file.Artist.Value.Should().NotBeNull();
        }

        [Test]
        public void get_files_by_artist_should_only_return_tracks_for_monitored_releases()
        {
            VerifyData();
            var artistFiles = Subject.GetFilesByArtist(_artist.Id);
            VerifyEagerLoaded(artistFiles);

            artistFiles.Should().HaveCount(4);
        }

        [Test]
        public void get_files_by_album()
        {
            VerifyData();
            var files = Subject.GetFilesByAlbum(_album.Id);
            VerifyEagerLoaded(files);

            files.Should().OnlyContain(c => c.AlbumId == _album.Id);
        }

        [Test]
        public void get_files_by_album_should_only_return_tracks_for_monitored_releases()
        {
            VerifyData();
            var files = Subject.GetFilesByAlbum(_album.Id);
            VerifyEagerLoaded(files);

            files.Should().HaveCount(4);
        }

        private void VerifyData()
        {
            Db.All<Artist>().Should().HaveCount(1);
            Db.All<Album>().Should().HaveCount(1);
            Db.All<Track>().Should().HaveCount(10);
            Db.All<TrackFile>().Should().HaveCount(10);
        }

        private void VerifyEagerLoaded(List<TrackFile> files)
        {
            foreach (var file in files)
            {
                file.Tracks.IsLoaded.Should().BeTrue();
                file.Tracks.Value.Should().NotBeNull();
                file.Tracks.Value.Should().NotBeEmpty();
                file.Album.IsLoaded.Should().BeTrue();
                file.Album.Value.Should().NotBeNull();
                file.Artist.IsLoaded.Should().BeTrue();
                file.Artist.Value.Should().NotBeNull();
                file.Artist.Value.Metadata.IsLoaded.Should().BeTrue();
                file.Artist.Value.Metadata.Value.Should().NotBeNull();
            }
        }

        private void VerifyUnmapped(List<TrackFile> files)
        {
            foreach (var file in files)
            {
                file.Tracks.IsLoaded.Should().BeFalse();
                file.Tracks.Value.Should().NotBeNull();
                file.Tracks.Value.Should().BeEmpty();
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

            Db.All<TrackFile>().Where(x => x.AlbumId == _album.Id).Should().HaveCount(0);
        }
    }
}
