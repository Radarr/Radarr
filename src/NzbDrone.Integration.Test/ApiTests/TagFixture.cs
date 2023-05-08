using FluentAssertions;
using NUnit.Framework;
using Radarr.Api.V3.Tags;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class TagFixture : IntegrationTest
    {
        [Test]
        [Order(0)]
        public void should_not_have_tags_initially()
        {
            EnsureNoTag("test");

            var items = Tags.All().Should().BeEmpty();
        }

        [Test]
        [Order(2)]
        public void should_be_able_to_add_tag()
        {
            var item = Tags.Post(new TagResource { Label = "test" });

            item.Id.Should().NotBe(0);
        }

        [Test]
        [Order(2)]
        public void get_all_tags()
        {
            EnsureTag("test");

            var clients = Tags.All();

            clients.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Order(4)]
        public void delete_tag()
        {
            var client = EnsureTag("test");

            Tags.Get(client.Id).Should().NotBeNull();

            Tags.Delete(client.Id);

            Tags.All().Should().NotContain(v => v.Id == client.Id);
        }
    }
}
