using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMovieFilesFixture : DbTest<CleanupOrphanedMovieFiles, MovieFile>
    {
        [Test]
        public void should_delete_orphaned_episode_files()
        {
            var episodeFile = Builder<MovieFile>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .BuildNew();

            Db.Insert(episodeFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_episode_files()
        {
            var episodeFiles = Builder<MovieFile>.CreateListOfSize(2)
                                                   .All()
                                                   .With(h => h.Quality = new QualityModel())
                                                   .BuildListOfNew();

            Db.InsertMany(episodeFiles);

            var episode = Builder<Movie>.CreateNew()
                                          .With(e => e.MovieFileId = episodeFiles.First().Id)
                                          .BuildNew();

            Db.Insert(episode);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            Db.All<Movie>().Should().Contain(e => e.MovieFileId == AllStoredModels.First().Id);
        }
    }
}
