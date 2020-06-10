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
        public void CleanInvalidCharacters(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }

        [TestCase(".45 (2006)",
            "45 (2006)")]
        public void CleanStartingDots(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }

        [TestCase(" The Movie Title ",
            "The Movie Title")]
        public void CleanSpaces(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }
    }
}
