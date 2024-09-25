using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]
    public class CleanFilenameFixture : CoreTest
    {
        [TestCase("Mission: Impossible - no [HDTV-720p]", "Mission - Impossible - no [HDTV-720p]")]
        public void should_replace_invalid_characters(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }

        [TestCase(".45 (2006)", "45 (2006)")]
        public void should_remove_periods_from_start(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }

        [TestCase(" The Movie Title", "The Movie Title")]
        [TestCase("The Movie Title ", "The Movie Title")]
        [TestCase(" The Movie Title ", "The Movie Title")]
        public void should_remove_spaces_from_start_and_end(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }
    }
}
