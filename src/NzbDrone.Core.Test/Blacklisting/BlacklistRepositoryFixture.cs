using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Blacklisting
{
    [TestFixture]
    public class BlacklistRepositoryFixture : DbTest<BlacklistRepository, Blacklist>
    {
        private Blacklist _blacklist;

        [SetUp]
        public void Setup()
        {
            _blacklist = new Blacklist
                     {
                         ArtistId = 12345,
                         AlbumIds = new List<int> { 1 },
                         Quality = new QualityModel(Quality.FLAC),
                         SourceTitle = "artist.name.album.title",
                         Date = DateTime.UtcNow
                     };
        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Subject.Insert(_blacklist);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_should_have_album_ids()
        {
            Subject.Insert(_blacklist);

            Subject.All().First().AlbumIds.Should().Contain(_blacklist.AlbumIds);
        }

        [Test]
        public void should_check_for_blacklisted_title_case_insensative()
        {
            Subject.Insert(_blacklist);

            Subject.BlacklistedByTitle(_blacklist.ArtistId, _blacklist.SourceTitle.ToUpperInvariant()).Should().HaveCount(1);
        }
    }
}
