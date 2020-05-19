using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class RawDiskSpecificationFixture : CoreTest<RawDiskSpecification>
    {
        private RemoteBook _remoteBook;

        [SetUp]
        public void Setup()
        {
            _remoteBook = new RemoteBook
            {
                Release = new ReleaseInfo() { DownloadProtocol = DownloadProtocol.Torrent }
            };
        }

        private void WithContainer(string container)
        {
            _remoteBook.Release.Container = container;
        }

        [Test]
        public void should_return_true_if_no_container_specified()
        {
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_flac()
        {
            WithContainer("FLAC");
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_vob()
        {
            WithContainer("VOB");
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_iso()
        {
            WithContainer("ISO");
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_compare_case_insensitive()
        {
            WithContainer("vob");
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }
    }
}
