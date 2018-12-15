using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class MediaFileRepositoryFixture : DbTest<MediaFileRepository, TrackFile>
    {
        [Test]
        public void get_files_by_artist()
        {

            var files = Builder<TrackFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality =new QualityModel(Quality.MP3_192))
                .BuildListOfNew();

            Db.InsertMany(files);
            Db.All<TrackFile>().Should().HaveCount(10);
            
            var artist = Builder<Artist>.CreateNew()
                .With(a => a.ArtistMetadataId = 11)
                .With(a => a.Id = 0)
                .Build();
            Db.Insert(artist);

            var album = Builder<Album>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.ArtistMetadataId = artist.ArtistMetadataId)
                .Build();
            Db.Insert(album);
            
            var release = Builder<AlbumRelease>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.AlbumId = album.Id)
                .With(a => a.Monitored = true)
                .Build();
            Db.Insert(release);
            
            var track = Builder<Track>.CreateListOfSize(10)
                .TheFirst(1)
                .With(a => a.TrackFileId = files[1].Id)
                .TheNext(1)
                .With(a => a.TrackFileId = files[2].Id)
                .TheNext(1)
                .With(a => a.TrackFileId = files[3].Id)
                .TheNext(1)
                .With(a => a.TrackFileId = files[4].Id)
                .TheNext(6)
                .With(a => a.TrackFileId = 0)
                .All()
                .With(a => a.Id = 0)
                .With(a => a.AlbumReleaseId = release.Id)
                .Build();
            Db.InsertMany(track);

            Db.All<Artist>().Should().HaveCount(1);
            Db.All<Album>().Should().HaveCount(1);
            Db.All<Track>().Should().HaveCount(10);

            var artistFiles = Subject.GetFilesByArtist(artist.Id);

            artistFiles.Should().HaveCount(4);

            artistFiles.Should().OnlyContain(c => c.ArtistId == artist.Id);
        }
    }
}
