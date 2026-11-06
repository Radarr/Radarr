using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMovieVersionFileIdsFixture : DbTest<CleanupOrphanedMovieVersionFileIds, MovieVersion>
    {
        [Test]
        public void should_remove_movie_file_id_pointing_to_a_missing_file()
        {
            var version = Builder<MovieVersion>.CreateNew()
                                               .With(v => v.MovieFileId = 42)
                                               .BuildNew();

            Db.Insert(version);
            Subject.Clean();

            AllStoredModels.Should().ContainSingle(v => v.MovieFileId == 0);
        }

        [Test]
        public void should_keep_movie_file_id_pointing_to_an_existing_file()
        {
            var movieFile = Builder<MovieFile>.CreateNew()
                                              .With(h => h.Quality = new QualityModel())
                                              .With(h => h.Languages = new List<Language> { Language.English })
                                              .BuildNew();

            Db.Insert(movieFile);

            var version = Builder<MovieVersion>.CreateNew()
                                               .With(v => v.MovieFileId = movieFile.Id)
                                               .BuildNew();

            Db.Insert(version);
            Subject.Clean();

            AllStoredModels.Should().ContainSingle(v => v.MovieFileId == movieFile.Id);
        }
    }
}
