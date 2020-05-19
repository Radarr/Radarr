using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class RetentionSpecificationFixture : CoreTest<RetentionSpecification>
    {
        private RemoteBook _remoteBook;

        [SetUp]
        public void Setup()
        {
            _remoteBook = new RemoteBook
            {
                Release = new ReleaseInfo() { DownloadProtocol = DownloadProtocol.Usenet }
            };
        }

        private void WithRetention(int days)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(days);
        }

        private void WithAge(int days)
        {
            _remoteBook.Release.PublishDate = DateTime.UtcNow.AddDays(-days);
        }

        [Test]
        public void should_return_true_when_retention_is_set_to_zero()
        {
            WithRetention(0);
            WithAge(100);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_release_if_younger_than_retention()
        {
            WithRetention(1000);
            WithAge(100);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_release_and_retention_are_the_same()
        {
            WithRetention(100);
            WithAge(100);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_old_than_retention()
        {
            WithRetention(10);
            WithAge(100);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_release_came_out_today_and_retention_is_zero()
        {
            WithRetention(0);
            WithAge(100);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_release_is_not_usenet()
        {
            _remoteBook.Release.DownloadProtocol = DownloadProtocol.Torrent;

            WithRetention(10);
            WithAge(100);

            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }
    }
}
