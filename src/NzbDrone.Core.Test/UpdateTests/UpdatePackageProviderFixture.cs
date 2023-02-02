using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Test.UpdateTests
{
    public class UpdatePackageProviderFixture : CoreTest<UpdatePackageProvider>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IPlatformInfo>().SetupGet(c => c.Version).Returns(new Version("9.9.9"));
        }

        [Test]
        public void no_update_when_version_higher()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("develop", new Version(10, 0)).Should().BeNull();
        }

        [Test]
        public void finds_update_when_version_lower()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("develop", new Version(3, 0)).Should().NotBeNull();
        }

        [Test]
        [Ignore("TODO: Update API")]
        public void should_get_master_if_branch_doesnt_exit()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("invalid_branch", new Version(0, 2)).Should().NotBeNull();
        }

        [Test]
        public void should_get_recent_updates()
        {
            const string branch = "nightly";
            UseRealHttp();
            var recent = Subject.GetRecentUpdates(branch, new Version(3, 0), null);
            var recentWithChanges = recent.Where(c => c.Changes != null);

            recent.Should().NotBeEmpty();
            recent.Should().OnlyContain(c => c.Hash.IsNotNullOrWhiteSpace());
            recent.Should().OnlyContain(c => c.FileName.Contains("Radarr"));
            recent.Should().OnlyContain(c => c.ReleaseDate.Year >= 2014);

            if (recentWithChanges.Any())
            {
                recentWithChanges.Should().OnlyContain(c => c.Changes.New != null);
                recentWithChanges.Should().OnlyContain(c => c.Changes.Fixed != null);
            }

            recent.Should().OnlyContain(c => c.Branch == branch);
        }
    }
}
