using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMovieMovieFileIdsFixture : DbTest<CleanupOrphanedMovieMovieFileIds, Movie>
    {
        [Test]
        public void should_remove_moviefileid_from_movie_referencing_deleted_moviefile()
        {
            var removedId = 2;

            var movie = Builder<Movie>.CreateNew()
                                          .With(e => e.MovieFileId = removedId)
                                          .BuildNew();

            Db.Insert(movie);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            Db.All<Movie>().Should().Contain(e => e.MovieFileId == 0);
        }

        [Test]
        public void should_not_remove_moviefileid_from_movie_referencing_valid_moviefile()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(2)
                                                   .All()
                                                   .With(h => h.Quality = new QualityModel())
                                                   .With(h => h.Languages = new List<Language> { Language.English })
                                                   .BuildListOfNew();

            Db.InsertMany(movieFiles);

            var movie = Builder<Movie>.CreateNew()
                                          .With(e => e.MovieFileId = movieFiles.First().Id)
                                          .BuildNew();

            Db.Insert(movie);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            Db.All<Movie>().Should().Contain(e => e.MovieFileId == movieFiles.First().Id);
        }
    }
}
