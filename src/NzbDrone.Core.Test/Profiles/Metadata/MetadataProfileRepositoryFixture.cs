using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Profiles.Metadata
{
    [TestFixture]
    public class MetadataProfileRepositoryFixture : DbTest<MetadataProfileRepository, MetadataProfile>
    {
        [Test]
        public void should_be_able_to_read_and_write()
        {
            // TODO: restore
        }
    }
}
