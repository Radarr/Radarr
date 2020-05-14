using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class MovieTitleSlugValidatorFixture : CoreTest<MovieTitleSlugValidator>
    {
        private List<Movie> _movies;
        private TestValidator<Movie> _validator;

        [SetUp]
        public void Setup()
        {
            _movies = Builder<Movie>.CreateListOfSize(1)
                                     .Build()
                                     .ToList();

            _validator = new TestValidator<Movie>
                            {
                                v => v.RuleFor(s => s.TitleSlug).SetValidator(Subject)
                            };

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetAllMovies())
                  .Returns(_movies);
        }

        [Test]
        public void should_not_be_valid_if_there_is_an_existing_movie_with_the_same_title_slug()
        {
            var movie = Builder<Movie>.CreateNew()
                                        .With(s => s.Id = 100)
                                        .With(s => s.TitleSlug = _movies.First().TitleSlug)
                                        .Build();

            _validator.Validate(movie).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_be_valid_if_there_is_not_an_existing_movie_with_the_same_title_slug()
        {
            var movie = Builder<Movie>.CreateNew()
                                        .With(s => s.TitleSlug = "MyTitleSlug")
                                        .Build();

            _validator.Validate(movie).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_valid_if_there_is_an_existing_movie_with_a_null_title_slug()
        {
            _movies.First().TitleSlug = null;

            var movie = Builder<Movie>.CreateNew()
                                        .With(s => s.TitleSlug = "MyTitleSlug")
                                        .Build();

            _validator.Validate(movie).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_valid_when_updating_an_existing_movie()
        {
            _validator.Validate(_movies.First().JsonClone()).IsValid.Should().BeTrue();
        }
    }
}
