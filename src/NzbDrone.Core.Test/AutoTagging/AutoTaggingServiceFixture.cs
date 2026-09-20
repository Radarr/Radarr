using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.AutoTagging.Specifications;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.AutoTagging
{
    [TestFixture]
    public class AutoTaggingServiceFixture : CoreTest<AutoTaggingService>
    {
        private Movie _movie;
        private AutoTag _tag;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                .With(m => m.MovieMetadata = new MovieMetadata
                {
                    Genres = new List<string> { "Comedy" }
                })
                .With(m => m.MovieFileId = 0)
                .With(m => m.MovieFile = null)
                .Build();

            _tag = new AutoTag
            {
                Name = "Test",
                Specifications = new List<IAutoTaggingSpecification>
                {
                    new GenreSpecification
                    {
                        Name = "Genre",
                        Value = new List<string>
                        {
                            "Comedy"
                        }
                    }
                },
                Tags = new HashSet<int> { 1 },
                RemoveTagsAutomatically = false
            };
        }

        private void GivenAutoTags(List<AutoTag> autoTags)
        {
            Mocker.GetMock<IAutoTaggingRepository>()
                  .Setup(s => s.All())
                  .Returns(autoTags);
        }

        [Test]
        public void should_not_have_changes_if_there_are_no_auto_tags()
        {
            GivenAutoTags(new List<AutoTag>());

            var result = Subject.GetTagChanges(_movie);

            result.TagsToAdd.Should().BeEmpty();
            result.TagsToRemove.Should().BeEmpty();
        }

        [Test]
        public void should_have_tags_to_add_if_series_does_not_have_match_tag()
        {
            GivenAutoTags(new List<AutoTag> { _tag });

            var result = Subject.GetTagChanges(_movie);

            result.TagsToAdd.Should().HaveCount(1);
            result.TagsToAdd.Should().Contain(1);
            result.TagsToRemove.Should().BeEmpty();
        }

        [Test]
        public void should_not_have_tags_to_remove_if_series_has_matching_tag_but_remove_is_false()
        {
            _movie.Tags = new HashSet<int> { 1 };
            _movie.MovieMetadata.Value.Genres = new List<string> { "NotComedy" };

            GivenAutoTags(new List<AutoTag> { _tag });

            var result = Subject.GetTagChanges(_movie);

            result.TagsToAdd.Should().BeEmpty();
            result.TagsToRemove.Should().BeEmpty();
        }

        [Test]
        public void should_have_tags_to_remove_if_series_has_matching_tag_and_remove_is_true()
        {
            _movie.Tags = new HashSet<int> { 1 };
            _movie.MovieMetadata.Value.Genres = new List<string> { "NotComedy" };

            _tag.RemoveTagsAutomatically = true;

            GivenAutoTags(new List<AutoTag> { _tag });

            var result = Subject.GetTagChanges(_movie);

            result.TagsToAdd.Should().BeEmpty();
            result.TagsToRemove.Should().HaveCount(1);
            result.TagsToRemove.Should().Contain(1);
        }

        [Test]
        public void should_match_if_specification_is_negated()
        {
            _movie.MovieMetadata.Value.Genres = new List<string> { "NotComedy" };

            _tag.Specifications.First().Negate = true;

            GivenAutoTags(new List<AutoTag> { _tag });

            var result = Subject.GetTagChanges(_movie);

            result.TagsToAdd.Should().HaveCount(1);
            result.TagsToAdd.Should().Contain(1);
            result.TagsToRemove.Should().BeEmpty();
        }

        [Test]
        public void should_add_tag_when_movie_file_matches_custom_format()
        {
            var movieFile = Builder<MovieFile>.CreateNew().With(f => f.Id = 10).Build();
            var customFormat = new CustomFormat { Id = 5, Name = "HDR" };

            _movie.MovieFileId = movieFile.Id;
            _movie.MovieFile = movieFile;
            _movie.Tags = new HashSet<int>();

            _tag.Specifications = new List<IAutoTaggingSpecification>
            {
                new CustomFormatSpecification
                {
                    Name = "Custom Format",
                    Value = customFormat.Id
                }
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(movieFile, _movie))
                  .Returns(new List<CustomFormat> { customFormat });

            GivenAutoTags(new List<AutoTag> { _tag });

            var result = Subject.GetTagChanges(_movie);

            result.TagsToAdd.Should().Contain(1);
            result.TagsToRemove.Should().BeEmpty();
        }

        [Test]
        public void should_remove_tag_when_movie_file_does_not_match_custom_format_and_remove_is_true()
        {
            var movieFile = Builder<MovieFile>.CreateNew().With(f => f.Id = 10).Build();

            _movie.MovieFileId = movieFile.Id;
            _movie.MovieFile = movieFile;
            _movie.Tags = new HashSet<int> { 1 };

            _tag.RemoveTagsAutomatically = true;
            _tag.Specifications = new List<IAutoTaggingSpecification>
            {
                new CustomFormatSpecification
                {
                    Name = "Custom Format",
                    Value = 5
                }
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(movieFile, _movie))
                  .Returns(new List<CustomFormat>());

            GivenAutoTags(new List<AutoTag> { _tag });

            var result = Subject.GetTagChanges(_movie);

            result.TagsToAdd.Should().BeEmpty();
            result.TagsToRemove.Should().Contain(1);
        }

        [Test]
        public void should_hydrate_movie_file_when_missing_for_custom_format_match()
        {
            var movieFile = Builder<MovieFile>.CreateNew().With(f => f.Id = 10).Build();
            var customFormat = new CustomFormat { Id = 5, Name = "HDR" };

            _movie.MovieFileId = movieFile.Id;
            _movie.MovieFile = null;
            _movie.Tags = new HashSet<int>();

            _tag.Specifications = new List<IAutoTaggingSpecification>
            {
                new CustomFormatSpecification
                {
                    Name = "Custom Format",
                    Value = customFormat.Id
                }
            };

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetMovie(movieFile.Id))
                  .Returns(movieFile);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(movieFile, _movie))
                  .Returns(new List<CustomFormat> { customFormat });

            GivenAutoTags(new List<AutoTag> { _tag });

            var result = Subject.GetTagChanges(_movie);

            result.TagsToAdd.Should().Contain(1);
            result.TagsToRemove.Should().BeEmpty();
            _movie.MovieFile.Should().Be(movieFile);
            _movie.MatchedCustomFormatIds.Should().BeNull();
        }
    }
}
