using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    public class SkyHookProxyMapMovieFixture : CoreTest<SkyHookProxy>
    {
        private MovieResource CreateResource(params AlternativeTitleResource[] alternativeTitles)
        {
            return new MovieResource
            {
                TmdbId = 617126,
                Title = "The Fantastic 4: First Steps",
                OriginalTitle = "The Fantastic 4: First Steps",
                OriginalLanguage = "en",
                Year = 2025,
                Overview = "Overview",
                Images = new List<ImageResource>(),
                Certifications = new List<CertificationResource>(),
                Translations = new List<TranslationResource>(),
                Ratings = new List<RatingItem> { new RatingItem { Count = 1, Value = 5 } },
                AlternativeTitles = new List<AlternativeTitleResource>(alternativeTitles)
            };
        }

        [Test]
        public void should_exclude_working_title_alternative_titles()
        {
            var resource = CreateResource(
                new AlternativeTitleResource { Title = "Blue Moon", Type = "working title", Language = "en" },
                new AlternativeTitleResource { Title = "Blue Moon FR", Type = "French working title", Language = "fr" },
                new AlternativeTitleResource { Title = "The Fantastic Four", Type = "alternative spelling", Language = "en" },
                new AlternativeTitleResource { Title = "Fantastic Four Promo", Type = "promotional title", Language = "en" });

            var movie = Subject.MapMovie(resource);

            movie.AlternativeTitles.Should().HaveCount(2);
            movie.AlternativeTitles.Should().Contain(t => t.Title == "The Fantastic Four");
            movie.AlternativeTitles.Should().Contain(t => t.Title == "Fantastic Four Promo");
            movie.AlternativeTitles.Should().NotContain(t => t.Title == "Blue Moon");
            movie.AlternativeTitles.Should().NotContain(t => t.Title == "Blue Moon FR");
        }

        [Test]
        public void should_keep_alternative_titles_without_type()
        {
            var resource = CreateResource(
                new AlternativeTitleResource { Title = "Some Title", Type = null, Language = "en" });

            var movie = Subject.MapMovie(resource);

            movie.AlternativeTitles.Should().HaveCount(1);
            movie.AlternativeTitles[0].Title.Should().Be("Some Title");
        }
    }
}
