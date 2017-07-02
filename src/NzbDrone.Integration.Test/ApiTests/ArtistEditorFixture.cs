using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ArtistEditorFixture : IntegrationTest
    {
        private void GivenExistingArtist()
        {
            foreach (var name in new[] { "90210", "Dexter" })
            {
                var newArtist = Artist.Lookup(name).First();

                newArtist.ProfileId = 1;
                newArtist.Path = string.Format(@"C:\Test\{0}", name).AsOsAgnostic();

                Artist.Post(newArtist);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_artist()
        {
            GivenExistingArtist();

            var artist = Artist.All();

            foreach (var s in artist)
            {
                s.ProfileId = 2;
            }

            var result = Artist.Editor(artist);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.ProfileId == 2).Should().BeTrue();
        }
    }
}