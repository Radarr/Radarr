﻿using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBlacklistFixture : DbTest<CleanupOrphanedBlacklist, Blacklist>
    {
        [Test]
        public void should_delete_orphaned_blacklist_items()
        {
            var blacklist = Builder<Blacklist>.CreateNew()
                                              .With(h => h.EpisodeIds = new List<int>())
                                              .With(h => h.Quality = new QualityModel())
                                              .BuildNew();

            Db.Insert(blacklist);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_blacklist_items()
        {
            var movie = Builder<Movie>.CreateNew().BuildNew();

            Db.Insert(movie);

            var blacklist = Builder<Blacklist>.CreateNew()
                                              .With(h => h.EpisodeIds = new List<int>())
                                              .With(h => h.Quality = new QualityModel())
                                              .With(b => b.MovieId = movie.Id)
                                              .BuildNew();

            Db.Insert(blacklist);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
