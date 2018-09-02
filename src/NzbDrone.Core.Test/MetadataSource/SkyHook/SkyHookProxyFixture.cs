using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxyFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [TestCase(11, "Star Wars")]
        [TestCase(2, "Ariel")]
        [TestCase(70981, "Prometheus")]
        public void should_be_able_to_get_movie_detail(int tmdbId, string title)
        {
            var details = Subject.GetMovieInfo(tmdbId);

            ValidateMovie(details);

            details.Title.Should().Be(title);
        }

        private void ValidateMovie(Movie movie)
        {
            movie.Should().NotBeNull();
            movie.Title.Should().NotBeNullOrWhiteSpace();
            movie.CleanTitle.Should().Be(Parser.Parser.CleanSeriesTitle(movie.Title));
            movie.SortTitle.Should().Be(MovieTitleNormalizer.Normalize(movie.Title, movie.TmdbId));
            movie.Overview.Should().NotBeNullOrWhiteSpace();
            movie.InCinemas.Should().HaveValue();
            movie.Images.Should().NotBeEmpty();
            movie.ImdbId.Should().NotBeNullOrWhiteSpace();
            movie.Studio.Should().NotBeNullOrWhiteSpace();
            movie.Runtime.Should().BeGreaterThan(0);
            movie.TitleSlug.Should().NotBeNullOrWhiteSpace();
            //series.TvRageId.Should().BeGreaterThan(0);
            movie.TmdbId.Should().BeGreaterThan(0);
        }
    }
}
