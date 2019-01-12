using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerSearchTests
{ 
    public class AlbumSearchDefinitionFixture : CoreTest<AlbumSearchCriteria>
    {
        [TestCase("Mötley Crüe", "Motley+Crue")]
        [TestCase("방탄소년단", "방탄소년단")]
        public void should_replace_some_special_characters_artist(string artist, string expected)
        {
            Subject.Artist = new Artist { Name = artist };
            Subject.ArtistQuery.Should().Be(expected);
        }

        [TestCase("…and Justice for All", "and+Justice+for+All")]
        [TestCase("American III: Solitary Man", "American+III+Solitary+Man")]
        [TestCase("Sad Clowns & Hillbillies", "Sad+Clowns+Hillbillies")]
        [TestCase("¿Quién sabe?", "Quien+sabe")]
        [TestCase("Seal the Deal & Let’s Boogie", "Seal+the+Deal+Lets+Boogie")]
        [TestCase("Section.80", "Section80")]
        public void should_replace_some_special_characters(string album, string expected)
        {
            Subject.AlbumTitle = album;
            Subject.AlbumQuery.Should().Be(expected);
        }
        
        [TestCase("+", "+")]
        public void should_not_replace_some_special_characters_if_result_empty_string(string album, string expected)
        {
            Subject.AlbumTitle = album;
            Subject.AlbumQuery.Should().Be(expected);
        }
    }
}
