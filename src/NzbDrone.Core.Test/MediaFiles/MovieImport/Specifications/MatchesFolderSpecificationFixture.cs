using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class MatchesFolderSpecificationFixture : CoreTest<MatchesFolderSpecification>
    {
        private Movie _movie;
        private Movie _otherMovie;
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                .With(m => m.Id = 1)
                .Build();

            _otherMovie = Builder<Movie>.CreateNew()
                .With(m => m.Id = 2)
                .Build();

            _localMovie = Builder<LocalMovie>.CreateNew()
                .With(l => l.Path = @"C:\Test\Unsorted\Movie.Title.2011.720p.BluRay-Radarr\Movie.Title.2011.720p.BluRay-Radarr.mkv".AsOsAgnostic())
                .With(l => l.Movie = _movie)
                .With(l => l.FolderMovieInfo = new ParsedMovieInfo
                {
                    MovieTitles = new List<string> { "Movie Title" },
                    Year = 2011
                })
                .With(l => l.FileMovieInfo = new ParsedMovieInfo
                {
                    MovieTitles = new List<string> { "Movie Title" },
                    Year = 2011
                })
                .Build();
        }

        private void GivenFileMapsTo(Movie movie)
        {
            Mocker.GetMock<IParsingService>()
                .Setup(s => s.Map(_localMovie.FileMovieInfo, It.IsAny<string>(), It.IsAny<int>(), null))
                .Returns(new RemoteMovie { Movie = movie });
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localMovie.ExistingFile = true;
            GivenFileMapsTo(_otherMovie);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_folder_name_is_not_parseable()
        {
            _localMovie.FolderMovieInfo = null;
            GivenFileMapsTo(_otherMovie);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_name_is_not_parseable()
        {
            _localMovie.FileMovieInfo = null;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_title_is_empty()
        {
            _localMovie.FileMovieInfo = new ParsedMovieInfo();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_does_not_map_to_a_movie()
        {
            GivenFileMapsTo(null);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_maps_to_the_same_movie()
        {
            GivenFileMapsTo(_movie);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_file_maps_to_a_different_movie()
        {
            GivenFileMapsTo(_otherMovie);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_pass_ids_from_file_to_the_parsing_service()
        {
            _localMovie.FileMovieInfo.ImdbId = "tt1111111";
            _localMovie.FileMovieInfo.TmdbId = 50000;

            GivenFileMapsTo(_movie);

            Subject.IsSatisfiedBy(_localMovie, null);

            Mocker.GetMock<IParsingService>()
                .Verify(s => s.Map(_localMovie.FileMovieInfo, "tt1111111", 50000, null), Times.Once());
        }
    }
}
