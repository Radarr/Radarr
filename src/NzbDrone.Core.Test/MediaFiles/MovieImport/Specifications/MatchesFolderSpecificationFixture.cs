using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class MatchesFolderSpecificationFixture : CoreTest<MatchesFolderSpecification>
    {
        private Movie _movie;
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                .With(m => m.MovieMetadata = new MovieMetadata
                {
                    Title = "Movie Title",
                    CleanTitle = "movietitle",
                    Year = 2011,
                    TmdbId = 50000,
                    ImdbId = "tt1111111"
                })
                .Build();

            _localMovie = Builder<LocalMovie>.CreateNew()
                .With(l => l.Path = @"C:\Test\Unsorted\Movie.Title.2011.720p.BluRay-Radarr\Movie.Title.2011.720p.BluRay-Radarr.mkv".AsOsAgnostic())
                .With(l => l.Movie = _movie)
                .With(l => l.FileMovieInfo = new ParsedMovieInfo
                {
                    MovieTitles = new List<string> { "Movie Title" },
                    Year = 2011
                })
                .Build();
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localMovie.ExistingFile = true;
            _localMovie.FileMovieInfo.Year = 2008;

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
        public void should_be_accepted_if_file_year_matches_movie()
        {
            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_year_matches_secondary_year()
        {
            _movie.MovieMetadata.Value.Year = 2012;
            _movie.MovieMetadata.Value.SecondaryYear = 2011;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_has_no_year()
        {
            _localMovie.FileMovieInfo.MovieTitles = new List<string> { "Another Movie" };
            _localMovie.FileMovieInfo.Year = 0;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_movie_has_no_year()
        {
            _movie.MovieMetadata.Value.Year = 0;
            _localMovie.FileMovieInfo.Year = 2008;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_year_is_part_of_the_title()
        {
            _movie.MovieMetadata.Value.Title = "Movie Title 2049";
            _movie.MovieMetadata.Value.CleanTitle = "movietitle2049";
            _movie.MovieMetadata.Value.Year = 2017;

            _localMovie.FileMovieInfo.MovieTitles = new List<string> { "Movie Title" };
            _localMovie.FileMovieInfo.Year = 2049;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_year_is_part_of_an_alternative_title()
        {
            _movie.MovieMetadata.Value.Year = 2017;
            _movie.MovieMetadata.Value.AlternativeTitles = new List<AlternativeTitle>
            {
                new AlternativeTitle
                {
                    Title = "Movie Title 2049",
                    CleanTitle = "movietitle2049"
                }
            };

            _localMovie.FileMovieInfo.Year = 2049;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_file_year_does_not_match_movie()
        {
            _localMovie.FileMovieInfo.Year = 2008;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_file_is_a_different_movie_with_a_different_year()
        {
            _localMovie.FileMovieInfo.MovieTitles = new List<string> { "Another Movie" };
            _localMovie.FileMovieInfo.Year = 2008;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_accepted_if_file_tmdb_id_matches_movie()
        {
            _localMovie.FileMovieInfo.TmdbId = 50000;
            _localMovie.FileMovieInfo.Year = 2008;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_file_tmdb_id_does_not_match_movie()
        {
            _localMovie.FileMovieInfo.TmdbId = 60000;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_accepted_if_file_imdb_id_matches_movie()
        {
            _localMovie.FileMovieInfo.ImdbId = "tt1111111";
            _localMovie.FileMovieInfo.Year = 2008;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_file_imdb_id_does_not_match_movie()
        {
            _localMovie.FileMovieInfo.ImdbId = "tt2222222";

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }
    }
}
