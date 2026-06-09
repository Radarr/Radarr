using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class SearchDefinitionFixture : CoreTest<MovieSearchCriteria>
    {
        [TestCase("Betty White's Off Their Rockers", "Betty+Whites+Off+Their+Rockers")]
        [TestCase("Star Wars: The Clone Wars", "Star+Wars+The+Clone+Wars")]
        [TestCase("Hawaii Five-0", "Hawaii+Five+0")]
        [TestCase("Franklin & Bash", "Franklin+and+Bash")]
        [TestCase("Chicago P.D.", "Chicago+P+D")]
        [TestCase("Kourtney And Khlo\u00E9 Take The Hamptons", "Kourtney+And+Khloe+Take+The+Hamptons")]
        [TestCase("Betty White`s Off Their Rockers", "Betty+Whites+Off+Their+Rockers")]
        [TestCase("Betty White\u00b4s Off Their Rockers", "Betty+Whites+Off+Their+Rockers")]
        [TestCase("Betty White‘s Off Their Rockers", "Betty+Whites+Off+Their+Rockers")]
        [TestCase("Betty White’s Off Their Rockers", "Betty+Whites+Off+Their+Rockers")]
        [TestCase("Snake Eyes: G.I. Joe Origins", "Snake+Eyes+G+I+Joe+Origins")]
        [TestCase("Sniper: G.R.I.T. - Global Response & Intelligence Team", "Sniper+G+R+I+T+Global+Response+and+Intelligence+Team")]
        [TestCase("Ghost in the Shell 2.0", "Ghost+in+the+Shell+2+0")]
        [TestCase("G.O.A.T", "G+O+A+T")]
        public void should_replace_some_special_characters(string input, string expected)
        {
            Subject.SceneTitles = new List<string> { input };
            Subject.CleanSceneTitles.First().Should().Be(expected);
        }
    }
}
