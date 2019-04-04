using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class MediaFileRepositoryFixture : DbTest<MediaFileRepository, TrackFile>
    {
        private Artist artist;
        private Album album;
        
        [SetUp]
        public void Setup()
        {
            var meta = Builder<ArtistMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(meta);
            
            artist = Builder<Artist>.CreateNew()
                .With(a => a.ArtistMetadataId = meta.Id)
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(artist);

            album = Builder<Album>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.ArtistMetadataId = artist.ArtistMetadataId)
                .Build();
            Db.Insert(album);
            
            var releases = Builder<AlbumRelease>.CreateListOfSize(2)
                .All()
                .With(a => a.Id = 0)
                .With(a => a.AlbumId = album.Id)
                .TheFirst(1)
                .With(a => a.Monitored = true)
                .TheNext(1)
                .With(a => a.Monitored = false)
                .Build();
            Db.InsertMany(releases);
            
            var files = Builder<TrackFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality =new QualityModel(Quality.MP3_192))
                .TheFirst(5)
                .With(c => c.AlbumId = album.Id)
                .BuildListOfNew();
            Db.InsertMany(files);
            
            var track = Builder<Track>.CreateListOfSize(10)
                .All()
                .With(a => a.Id = 0)
                .TheFirst(4)
                .With(a => a.AlbumReleaseId = releases[0].Id)
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
                .With(a => a.AlbumReleaseId = releases[1].Id)
                .TheNext(5)
                .With(a => a.TrackFileId = 0)
                .Build();
            Db.InsertMany(track);
        }
        
        [Test]
        public void get_files_by_artist()
        {
            VerifyData();
            var artistFiles = Subject.GetFilesByArtist(artist.Id);
            VerifyEagerLoaded(artistFiles);

            artistFiles.Should().OnlyContain(c => c.Artist.Value.Id == artist.Id);
        }

        [Test]
        public void get_files_by_artist_should_only_return_tracks_for_monitored_releases()
        {
            VerifyData();
            var artistFiles = Subject.GetFilesByArtist(artist.Id);
            VerifyEagerLoaded(artistFiles);

            artistFiles.Should().HaveCount(4);
        }

        [Test]
        public void get_files_by_album()
        {
            VerifyData();
            var files = Subject.GetFilesByAlbum(album.Id);
            VerifyEagerLoaded(files);

            files.Should().OnlyContain(c => c.AlbumId == album.Id);
        }

        [Test]
        public void get_files_by_album_should_only_return_tracks_for_monitored_releases()
        {
            VerifyData();
            var files = Subject.GetFilesByAlbum(album.Id);
            VerifyEagerLoaded(files);
            
            files.Should().HaveCount(4);
        }

        [Test]
        public void get_files_by_relative_path()
        {
            VerifyData();
            var files = Subject.GetFilesWithRelativePath(artist.Id, "RelativePath2");
            VerifyEagerLoaded(files);
            
            files.Should().OnlyContain(c => c.AlbumId == album.Id);
            files.Should().OnlyContain(c => c.RelativePath == "RelativePath2");
        }
        
        [Test]
        public void get_files_by_relative_path_should_only_contain_monitored_releases()
        {
            VerifyData();
            
            // file 5 is linked to an unmonitored release
            var files = Subject.GetFilesWithRelativePath(artist.Id, "RelativePath5");
            
            files.Should().BeEmpty();
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
    }
}
