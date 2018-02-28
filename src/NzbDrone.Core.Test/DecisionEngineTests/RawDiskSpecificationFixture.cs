using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;

using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class RawDiskSpecificationFixture : CoreTest<RawDiskSpecification>
    {
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            _remoteMovie = new RemoteMovie
            {
                Release = new ReleaseInfo() { DownloadProtocol = DownloadProtocol.Torrent }
            };
        }

        private void WithContainer(string container)
        {
            _remoteMovie.Release.Container = container;
        }
        
        [Test]
        public void should_return_true_if_no_container_specified()
        {
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_mkv()
        {
            WithContainer("MKV");
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_vob()
        {
            WithContainer("VOB");
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_iso()
        {
            WithContainer("ISO");
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_m2ts()
        {
            WithContainer("M2TS");
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_compare_case_insensitive()
        {
            WithContainer("vob");
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

    }
}
