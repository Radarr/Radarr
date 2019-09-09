using FluentAssertions;
using NUnit.Framework;
using Lidarr.Api.V1.Artist;
using System.Linq;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ArtistEditorFixture : IntegrationTest
    {
        private void GivenExistingArtist()
        {
            foreach (var name in new[] { "Alien Ant Farm", "Kiss" })
            {
                var newArtist = Artist.Lookup(name).First();

                newArtist.QualityProfileId = 1;
                newArtist.MetadataProfileId = 1;
                newArtist.Path = string.Format(@"C:\Test\{0}", name).AsOsAgnostic();

                Artist.Post(newArtist);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_artist()
        {
            GivenExistingArtist();

            var artist = Artist.All();

            var artistEditor = new ArtistEditorResource
            {
                QualityProfileId = 2,
                ArtistIds = artist.Select(o => o.Id).ToList()
            };

            var result = Artist.Editor(artistEditor);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.QualityProfileId == 2).Should().BeTrue();
        }
    }
}
