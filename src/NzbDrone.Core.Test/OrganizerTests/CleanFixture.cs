using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]
    public class CleanFixture : CoreTest
    {
        [TestCase("Mission: Impossible - no [HDTV-720p]",
            "Mission Impossible - no [HDTV-720p]")]
        public void CleanFileName(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name, NamingConfig.Default).Should().Be(expectedName);
        }

    }
}
