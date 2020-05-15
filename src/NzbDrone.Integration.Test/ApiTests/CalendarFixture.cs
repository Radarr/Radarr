using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Integration.Test.Client;
using Readarr.Api.V1.Books;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class CalendarFixture : IntegrationTest
    {
        public ClientBase<BookResource> Calendar;

        protected override void InitRestClients()
        {
            base.InitRestClients();

            Calendar = new ClientBase<BookResource>(RestClient, ApiKey, "calendar");
        }

        [Test]
        public void should_be_able_to_get_albums()
        {
            var artist = EnsureAuthor("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", true);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2003, 06, 20).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2003, 06, 22).ToString("s") + "Z");
            var items = Calendar.Get<List<BookResource>>(request);

            items = items.Where(v => v.AuthorId == artist.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("Harry Potter and the Order of the Phoenix");
        }

        [Test]
        public void should_not_be_able_to_get_unmonitored_albums()
        {
            var artist = EnsureAuthor("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2003, 06, 20).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2003, 06, 22).ToString("s") + "Z");
            request.AddParameter("unmonitored", "false");
            var items = Calendar.Get<List<BookResource>>(request);

            items = items.Where(v => v.AuthorId == artist.Id).ToList();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_get_unmonitored_albums()
        {
            var artist = EnsureAuthor("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(2003, 06, 20).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(2003, 06, 22).ToString("s") + "Z");
            request.AddParameter("unmonitored", "true");
            var items = Calendar.Get<List<BookResource>>(request);

            items = items.Where(v => v.AuthorId == artist.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("Harry Potter and the Order of the Phoenix");
        }
    }
}
