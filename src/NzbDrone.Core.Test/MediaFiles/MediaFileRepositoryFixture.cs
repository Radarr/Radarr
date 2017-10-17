using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
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
                .Random(4)
                .With(s => s.ArtistId = 12)
                .BuildListOfNew();


            Db.InsertMany(files);

            var artistFiles = Subject.GetFilesByArtist(12);

            artistFiles.Should().HaveCount(4);
            artistFiles.Should().OnlyContain(c => c.ArtistId == 12);

        }
    }
}
