using FluentAssertions;
using NUnit.Framework;
using Lidarr.Api.V1.Albums;
using NzbDrone.Integration.Test.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class CalendarFixture : IntegrationTest
    {
        public ClientBase<AlbumResource> Calendar;

        protected override void InitRestClients()
        {
            base.InitRestClients();

            Calendar = new ClientBase<AlbumResource>(RestClient, ApiKey, "calendar");
        }

        [Test]
        public void should_be_able_to_get_albums()
        {
            var artist = EnsureArtist("aaaa_aaaaa_asaaaaa", "Alien Ant Farm", true);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 10, 3).ToString("s") + "Z");
            var items = Calendar.Get<List<AlbumResource>>(request);

            items = items.Where(v => v.ArtistId == artist.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("The Troll Farmer");
        }

        [Test]
        public void should_not_be_able_to_get_unmonitored_albums()
        {
            var artist = EnsureArtist("aaaa_aaaaa_asaaaaa", "Alien Ant Farm", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 10, 3).ToString("s") + "Z");
            request.AddParameter("unmonitored", "false");
            var items = Calendar.Get<List<AlbumResource>>(request);

            items = items.Where(v => v.ArtistId == artist.Id).ToList();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_get_unmonitored_albums()
        {
            var artist = EnsureArtist("aaaa_aaaaa_asaaaaa", "Alien Ant Farm", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 10, 3).ToString("s") + "Z");
            request.AddParameter("unmonitored", "true");
            var items = Calendar.Get<List<AlbumResource>>(request);

            items = items.Where(v => v.ArtistId == artist.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("The Troll Farmer");
        }
    }
}
