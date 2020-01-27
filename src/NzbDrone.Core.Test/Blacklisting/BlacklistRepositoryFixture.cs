using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Languages;
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
                MovieId = 1234,
                Quality = new QualityModel(),
                Languages = new List<Language>(),
                SourceTitle = "movie.title.1998",
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
        public void should_should_have_movie_id()
        {
            Subject.Insert(_blacklist);

            Subject.All().First().MovieId.Should().Be(_blacklist.MovieId);
        }

        [Test]
        public void should_check_for_blacklisted_title_case_insensative()
        {
            Subject.Insert(_blacklist);

            Subject.BlacklistedByTitle(_blacklist.MovieId, _blacklist.SourceTitle.ToUpperInvariant()).Should().HaveCount(1);
        }
    }
}
