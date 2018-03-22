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

        [TestCase(75978, "Family Guy")]
        [TestCase(83462, "Castle (2009)")]
        [TestCase(266189, "The Blacklist")]
        public void should_be_able_to_get_movie_detail(int tmdbId, string title)
        {
            var details = Subject.GetMovieInfo(tmdbId);

            ValidateMovie(details);

            details.Title.Should().Be(title);
        }

        [Test]
        public void getting_details_of_invalid_series()
        {
            Assert.Throws<MovieNotFoundException>(() => Subject.GetMovieInfo(int.MaxValue));
        }

        [Test]
        public void should_not_have_period_at_start_of_title_slug()
        {
            var details = Subject.GetMovieInfo(79099);

            details.TitleSlug.Should().Be("dothack");
        }

        private void ValidateMovie(Movie movie)
        {
            movie.Should().NotBeNull();
            movie.Title.Should().NotBeNullOrWhiteSpace();
            movie.CleanTitle.Should().Be(Parser.Parser.CleanSeriesTitle(movie.Title));
            movie.SortTitle.Should().Be(MovieTitleNormalizer.Normalize(movie.Title, movie.TmdbId));
            movie.Overview.Should().NotBeNullOrWhiteSpace();
            movie.PhysicalRelease.Should().HaveValue();
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
