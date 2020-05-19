using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    public class MaximumSizeSpecificationFixture : CoreTest<MaximumSizeSpecification>
    {
        private RemoteBook _remoteBook;

        [SetUp]
        public void Setup()
        {
            _remoteBook = new RemoteBook() { Release = new ReleaseInfo() };
        }

        private void WithMaximumSize(int size)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.MaximumSize).Returns(size);
        }

        private void WithSize(int size)
        {
            _remoteBook.Release.Size = size * 1024 * 1024;
        }

        [Test]
        public void should_return_true_when_maximum_size_is_set_to_zero()
        {
            WithMaximumSize(0);
            WithSize(1000);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_size_is_smaller_than_maximum_size()
        {
            WithMaximumSize(2000);
            WithSize(1999);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_size_is_equals_to_maximum_size()
        {
            WithMaximumSize(2000);
            WithSize(2000);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_size_is_bigger_than_maximum_size()
        {
            WithMaximumSize(2000);
            WithSize(2001);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_size_is_zero()
        {
            WithMaximumSize(2000);
            WithSize(0);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }
    }
}
