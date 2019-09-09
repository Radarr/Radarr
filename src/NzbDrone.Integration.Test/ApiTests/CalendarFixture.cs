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
            var artist = EnsureArtist("cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493", "Adele", true);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 11, 19).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 11, 21).ToString("s") + "Z");
            var items = Calendar.Get<List<AlbumResource>>(request);

            items = items.Where(v => v.ArtistId == artist.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("25");
        }

        [Test]
        public void should_not_be_able_to_get_unmonitored_albums()
        {
            var artist = EnsureArtist("cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493", "Adele", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 11, 19).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 11, 21).ToString("s") + "Z");
            request.AddParameter("unmonitored", "false");
            var items = Calendar.Get<List<AlbumResource>>(request);

            items = items.Where(v => v.ArtistId == artist.Id).ToList();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_get_unmonitored_albums()
        {
            var artist = EnsureArtist("cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493", "Adele", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2015, 11, 19).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2015, 11, 21).ToString("s") + "Z");
            request.AddParameter("unmonitored", "true");
            var items = Calendar.Get<List<AlbumResource>>(request);

            items = items.Where(v => v.ArtistId == artist.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("25");
        }
    }
}
