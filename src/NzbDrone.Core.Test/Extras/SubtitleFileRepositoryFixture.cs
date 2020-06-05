using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class SubtitleFileRepositoryFixture : DbTest<SubtitleFileRepository, SubtitleFile>
    {
        private Movie _movie;
        private MovieFile _movieFile1;
        private MovieFile _movieFile2;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Id = 7)
                                     .Build();

            _movieFile1 = Builder<MovieFile>.CreateNew()
                                     .With(s => s.Id = 10)
                                     .With(s => s.MovieId = _movie.Id)
                                     .Build();

            _movieFile2 = Builder<MovieFile>.CreateNew()
                                     .With(s => s.Id = 11)
                                     .With(s => s.MovieId = _movie.Id)
                                     .Build();
        }

        [Test]
        public void should_delete_files_by_movieId()
        {
            var files = Builder<SubtitleFile>.CreateListOfSize(5)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.MovieId = _movie.Id)
                .With(c => c.MovieFileId = 11)
                .With(c => c.Language = Language.English)
                .BuildListOfNew();

            Db.InsertMany(files);

            Subject.DeleteForMovies(new List<int> { _movie.Id });

            var remainingFiles = Subject.GetFilesByMovie(_movie.Id);

            remainingFiles.Should().HaveCount(0);
        }

        [Test]
        public void should_delete_files_by_movieFileId()
        {
            var files = Builder<SubtitleFile>.CreateListOfSize(5)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.MovieId = _movie.Id)
                .With(c => c.MovieFileId = _movieFile2.Id)
                .With(c => c.Language = Language.English)
                .Random(2)
                .With(c => c.MovieFileId = _movieFile1.Id)
                .BuildListOfNew();

            Db.InsertMany(files);

            Subject.DeleteForMovieFile(_movieFile2.Id);

            var remainingFiles = Subject.GetFilesByMovie(_movie.Id);

            remainingFiles.Should().HaveCount(2);
        }

        [Test]
        public void should_get_files_by_movieFileId()
        {
            var files = Builder<SubtitleFile>.CreateListOfSize(5)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.MovieId = _movie.Id)
                .With(c => c.MovieFileId = _movieFile2.Id)
                .With(c => c.Language = Language.English)
                .Random(2)
                .With(c => c.MovieFileId = _movieFile1.Id)
                .BuildListOfNew();

            Db.InsertMany(files);

            var remainingFiles = Subject.GetFilesByMovieFile(_movieFile2.Id);

            remainingFiles.Should().HaveCount(3);
            remainingFiles.Should().OnlyContain(c => c.MovieFileId == _movieFile2.Id);
        }

        [Test]
        public void should_get_files_by_movieId()
        {
            var files = Builder<SubtitleFile>.CreateListOfSize(5)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.MovieId = _movie.Id)
                .With(c => c.MovieFileId = _movieFile2.Id)
                .With(c => c.Language = Language.English)
                .Random(2)
                .With(c => c.MovieFileId = _movieFile1.Id)
                .BuildListOfNew();

            Db.InsertMany(files);

            var remainingFiles = Subject.GetFilesByMovie(_movie.Id);

            remainingFiles.Should().HaveCount(5);
            remainingFiles.Should().OnlyContain(c => c.MovieId == _movie.Id);
        }
    }
}
