using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedExtraFilesFixture : DbTest<CleanupOrphanedExtraFiles, OtherExtraFile>
    {
        [Test]
        public void should_delete_extra_files_that_dont_have_a_coresponding_movie()
        {
            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                    .With(m => m.MovieFileId = 0)
                                                    .BuildNew();

            Db.Insert(extraFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_extra_files_that_have_a_coresponding_movie()
        {
            var movie = Builder<Movie>.CreateNew()
                                      .BuildNew();

            Db.Insert(movie);

            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                    .With(m => m.MovieId = movie.Id)
                                                    .With(m => m.MovieFileId = 0)
                                                    .BuildNew();

            Db.Insert(extraFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_extra_files_that_dont_have_a_coresponding_movie_file()
        {
            var movie = Builder<Movie>.CreateNew()
                                      .BuildNew();

            Db.Insert(movie);

            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                    .With(m => m.MovieId = movie.Id)
                                                    .With(m => m.MovieFileId = 10)
                                                    .BuildNew();

            Db.Insert(extraFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_extra_files_that_have_a_coresponding_movie_file()
        {
            var movie = Builder<Movie>.CreateNew()
                                      .BuildNew();

            var movieFile = Builder<MovieFile>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.Languages = new List<Language>())
                                                  .BuildNew();

            Db.Insert(movie);
            Db.Insert(movieFile);

            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                    .With(m => m.MovieId = movie.Id)
                                                    .With(m => m.MovieFileId = movieFile.Id)
                                                    .BuildNew();

            Db.Insert(extraFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
