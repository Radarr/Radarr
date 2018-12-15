using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    [TestFixture]
    public class ArtistSearchServiceFixture : CoreTest<ArtistSearchService>
    {
        private Artist _artist;

        [SetUp]
        public void Setup()
        {
            _artist = new Artist();

            Mocker.GetMock<IArtistService>()
                .Setup(s => s.GetArtist(It.IsAny<int>()))
                .Returns(_artist);

            Mocker.GetMock<ISearchForNzb>()
                .Setup(s => s.ArtistSearch(_artist.Id, false, true, false))
                .Returns(new List<DownloadDecision>());

            Mocker.GetMock<IProcessDownloadDecisions>()
                .Setup(s => s.ProcessDecisions(It.IsAny<List<DownloadDecision>>()))
                .Returns(new ProcessedDecisions(new List<DownloadDecision>(), new List<DownloadDecision>(),
                    new List<DownloadDecision>()));
        }

        [Test]
        public void should_only_include_monitored_albums()
        {
            _artist.Albums = new List<Album>
            {
                new Album {Monitored = false},
                new Album {Monitored = true}
            };

            Subject.Execute(new ArtistSearchCommand {ArtistId = _artist.Id, Trigger = CommandTrigger.Manual});

            Mocker.GetMock<ISearchForNzb>()
                .Verify(v => v.ArtistSearch(_artist.Id, false, true, false),
                    Times.Exactly(_artist.Albums.Value.Count(s => s.Monitored)));
        }
    }
}
