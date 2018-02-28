using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class MediaFileRepositoryFixture : DbTest<MediaFileRepository, MovieFile>
    {
        [Test]
        public void get_files_by_series()
        {
            var files = Builder<MovieFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality =new QualityModel(Quality.Bluray720p))
                .Random(4)
                .With(s => s.MovieId = 12)
                .BuildListOfNew();


            Db.InsertMany(files);

            var seriesFiles = Subject.GetFilesByMovie(12);

            seriesFiles.Should().HaveCount(4);
            seriesFiles.Should().OnlyContain(c => c.MovieId == 12);

        }
    }
}
