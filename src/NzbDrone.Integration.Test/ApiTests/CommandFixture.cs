using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Commands;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class CommandFixture : IntegrationTest
    {
        [Test]
        public void should_be_able_to_run_rss_sync()
        {
            var response = Commands.Post(new CommandResource { Name = "rsssync" });

            response.Id.Should().NotBe(0);
        }
    }
}
