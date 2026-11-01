using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class PosterReplacementServiceFixture : CoreTest<PosterReplacementService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                .With(m => m.Id = 1)
                .With(m => m.Tags = new HashSet<int> { 3 })
                .With(m => m.MovieMetadata.Value.Title = "The Thing")
                .With(m => m.MovieMetadata.Value.Genres = new List<string> { "Horror", "Science Fiction" })
                .Build();

            GivenSettings(true, new List<string>(), new List<int>());
        }

        private void GivenSettings(bool enabled, List<string> genres, List<int> tags)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementEnabled).Returns(enabled);
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementGenres).Returns(genres);
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementTags).Returns(tags);
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementBackgroundColor).Returns("#000000");
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementTextColor).Returns("#ffffff");
        }

        [Test]
        public void should_not_replace_when_disabled()
        {
            GivenSettings(false, new List<string> { "Horror" }, new List<int> { 3 });

            Subject.GetReplacement(_movie).Should().BeNull();
        }

        [Test]
        public void should_not_replace_when_no_genres_or_tags_are_configured()
        {
            Subject.GetReplacement(_movie).Should().BeNull();
        }

        [TestCase("Horror")]
        [TestCase("horror")]
        [TestCase("SCIENCE FICTION")]
        public void should_replace_when_genre_matches(string genre)
        {
            GivenSettings(true, new List<string> { "Thriller", genre }, new List<int>());

            var replacement = Subject.GetReplacement(_movie);

            replacement.Should().NotBeNull();
            replacement.Title.Should().Be("The Thing");
            replacement.BackgroundColor.Should().Be("#000000");
            replacement.TextColor.Should().Be("#ffffff");
        }

        [Test]
        public void should_not_replace_when_genre_does_not_match()
        {
            GivenSettings(true, new List<string> { "Comedy" }, new List<int> { 4 });

            Subject.GetReplacement(_movie).Should().BeNull();
        }

        [Test]
        public void should_replace_when_tag_matches()
        {
            GivenSettings(true, new List<string> { "Comedy" }, new List<int> { 3 });

            Subject.GetReplacement(_movie).Should().NotBeNull();
        }

        [Test]
        public void should_change_signature_when_colors_or_title_change()
        {
            GivenSettings(true, new List<string> { "Horror" }, new List<int>());

            var original = Subject.GetReplacement(_movie).Signature;

            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementBackgroundColor).Returns("#550000");
            var recolored = Subject.GetReplacement(_movie).Signature;

            _movie.MovieMetadata.Value.Title = "The Thing (Remastered)";
            var retitled = Subject.GetReplacement(_movie).Signature;

            new[] { original, recolored, retitled }.Should().OnlyHaveUniqueItems();
            Subject.GetReplacement(_movie).Signature.Should().Be(retitled);
        }
    }
}
