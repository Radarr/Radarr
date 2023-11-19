using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats
{
    [TestFixture]
    public class IndexerSpecificationFixture : CoreTest<IndexerSpecification>
    {
        private CustomFormatInput _movieInput;

        [SetUp]
        public void Setup()
        {
            _movieInput = new CustomFormatInput
            {
                Movie = Builder<Movie>.CreateNew().Build()
            };
        }

        private void GivenCustomFormatIndexer(int indexerId)
        {
            Subject.Value = indexerId;
        }

        private void GivenMovieReleaseIndexer(int indexerId)
        {
            _movieInput.IndexerId = indexerId;
        }

        [Test]
        public void should_return_false_if_indexer_id_not_captured()
        {
            GivenCustomFormatIndexer(4);
            GivenMovieReleaseIndexer(-1);

            Subject.IsSatisfiedBy(_movieInput).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_indexer_id_does_not_match_custom_format()
        {
            GivenCustomFormatIndexer(4);
            GivenMovieReleaseIndexer(3);

            Subject.IsSatisfiedBy(_movieInput).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_indexer_id_matches_custom_format()
        {
            GivenCustomFormatIndexer(4);
            GivenMovieReleaseIndexer(4);

            Subject.IsSatisfiedBy(_movieInput).Should().BeTrue();
        }
    }
}
