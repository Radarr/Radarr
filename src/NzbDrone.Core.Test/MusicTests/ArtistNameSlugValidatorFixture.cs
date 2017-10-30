using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation.Validators;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class ArtistNameSlugValidatorFixture : CoreTest<ArtistSlugValidator>
    {
        private List<Artist> _artist;
        private TestValidator<Artist> _validator;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateListOfSize(1)
                                     .Build()
                                     .ToList();

            _validator = new TestValidator<Artist>
                            {
                                v => v.RuleFor(s => s.NameSlug).SetValidator(Subject)
                            };

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetAllArtists())
                  .Returns(_artist);
        }

        [Test]
        public void should_not_be_valid_if_there_is_an_existing_artist_with_the_same_title_slug()
        {
            var series = Builder<Artist>.CreateNew()
                                        .With(s => s.Id = 100)
                                        .With(s => s.NameSlug = _artist.First().NameSlug)
                                        .Build();

            _validator.Validate(series).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_be_valid_if_there_is_not_an_existing_artist_with_the_same_title_slug()
        {
            var series = Builder<Artist>.CreateNew()
                                        .With(s => s.NameSlug = "MyNameSlug")
                                        .Build();

            _validator.Validate(series).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_valid_if_there_is_an_existing_artist_with_a_null_title_slug()
        {
            _artist.First().NameSlug = null;

            var series = Builder<Artist>.CreateNew()
                                        .With(s => s.NameSlug = "MyNameSlug")
                                        .Build();

            _validator.Validate(series).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_valid_when_updating_an_existing_artist()
        {
            _validator.Validate(_artist.First().JsonClone()).IsValid.Should().BeTrue();
        }
    }
}
