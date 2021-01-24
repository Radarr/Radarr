using FluentAssertions;
using NUnit.Framework;
using Readarr.Api.V1.Author;
using Readarr.Api.V1.Blacklist;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class BlacklistFixture : IntegrationTest
    {
        private AuthorResource _author;

        [Test]
        [Ignore("Adding to blacklist not supported")]
        public void should_be_able_to_add_to_blacklist()
        {
            _author = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");

            Blacklist.Post(new BlacklistResource
            {
                AuthorId = _author.Id,
                SourceTitle = "Blacklist - Book 1 [2015 FLAC]"
            });
        }

        [Test]
        [Ignore("Adding to blacklist not supported")]
        public void should_be_able_to_get_all_blacklisted()
        {
            var result = Blacklist.GetPaged(0, 1000, "date", "desc");

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(1);
            result.Records.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Ignore("Adding to blacklist not supported")]
        public void should_be_able_to_remove_from_blacklist()
        {
            Blacklist.Delete(1);

            var result = Blacklist.GetPaged(0, 1000, "date", "desc");

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(0);
        }
    }
}
