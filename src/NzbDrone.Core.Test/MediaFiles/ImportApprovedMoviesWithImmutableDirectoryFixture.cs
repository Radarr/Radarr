using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]

    public class ImportMoviesWithImmutableDirectoryFixture : CoreTest<ImportApprovedMovie>
    {
        private List<ImportDecision> _rejectedDecisions;
        private List<ImportDecision> _approvedDecisions;

        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _rejectedDecisions = new List<ImportDecision>();
            _approvedDecisions = new List<ImportDecision>();

            var movieName = "Test Movie (2000)";
            var outputPath = $"C:\\Test\\Unsorted\\Movies\\{movieName}".AsOsAgnostic();

            var movie = Builder<Movie>.CreateNew()
                .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                .With(s => s.Path = $"C:\\Test\\Movies\\{movieName}".AsOsAgnostic())
                .Build();

            _rejectedDecisions.Add(new ImportDecision(new LocalMovie(), new Rejection("Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalMovie(), new Rejection("Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalMovie(), new Rejection("Rejected!")));

            _approvedDecisions.Add(new ImportDecision(
                                       new LocalMovie
                                       {
                                           Movie = movie,
                                           Path = Path.Combine(movie.Path, "VIDEO_TS", "VTS_01_0.VOB"),
                                           Quality = new QualityModel(),
                                           ReleaseGroup = "DRONE"
                                       }));
            _approvedDecisions.Add(new ImportDecision(
                                       new LocalMovie
                                       {
                                           Movie = movie,
                                           Path = Path.Combine(movie.Path, "VIDEO_TS", "VTS_02_0.BUP"),
                                           Quality = new QualityModel(),
                                           ReleaseGroup = "DRONE"
                                       }));
            _approvedDecisions.Add(new ImportDecision(
                                       new LocalMovie
                                       {
                                           Movie = movie,
                                           Path = Path.Combine(movie.Path, "VIDEO_TS", "VTS_02_0.IFO"),
                                           Quality = new QualityModel(),
                                           ReleaseGroup = "DRONE"
                                       }));
            _approvedDecisions.Add(new ImportDecision(
                                       new LocalMovie
                                       {
                                           Movie = movie,
                                           Path = Path.Combine(movie.Path, "VIDEO_TS", "Anything.At.All"),
                                           Quality = new QualityModel(),
                                           ReleaseGroup = "DRONE"
                                       }));

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Setup(s => s.UpgradeMovieFile(It.IsAny<MovieFile>(), It.IsAny<LocalMovie>(), It.IsAny<bool>()))
                  .Returns(new MovieFileMoveResult());

            Mocker.GetMock<IHistoryService>()
                .Setup(x => x.FindByDownloadId(It.IsAny<string>()))
                .Returns(new List<MovieHistory>());

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .With(d => d.OutputPath = new OsPath(outputPath))
                                                             .Build();
        }

        [Test]
        public void should_not_import_any_if_there_are_no_approved_decisions()
        {
            Subject.Import(_rejectedDecisions, false).Where(i => i.Result == ImportResultType.Imported).Should().BeEmpty();

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.IsAny<MovieFile>()), Times.Never());
        }

        [Test]
        public void should_import_each_approved()
        {
            Subject.Import(_approvedDecisions, false).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_only_import_approved()
        {
            var all = new List<ImportDecision>();
            all.AddRange(_rejectedDecisions);
            all.AddRange(_approvedDecisions);

            var result = Subject.Import(all, false);

            result.Should().HaveCount(all.Count);
            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_move_new_immutable_downloads()
        {
            Subject.Import(_approvedDecisions, true);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeMovieFile(It.IsAny<MovieFile>(), _approvedDecisions.First().LocalMovie, false),
                          Times.Once());
        }

        [Test]
        public void should_publish_MovieImportedEvent_for_each_file_for_new_download()
        {
            Subject.Import(_approvedDecisions, true);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<MovieImportedEvent>()), Times.Exactly(_approvedDecisions.Count));
        }

        [Test]
        public void should_not_move_existing_files()
        {
            Subject.Import(_approvedDecisions, false);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeMovieFile(It.IsAny<MovieFile>(), _approvedDecisions.First().LocalMovie, false),
                          Times.Never());
        }

        [Test]
        public void should_indicate_immutable_directory_for_VIDEO_TS()
        {
            string path = Path.Combine(@"C:\Movies", "Movie Name (2000)", "VIDEO_TS", "VTS_01_0.VOB").AsOsAgnostic();
            new LocalMovie { Path = path }.IsImmutableSubdirectory.Should().BeTrue();

            path = Path.Combine(@"C:\Movies", "Movie Name (2000)", "VIDEO_TS", "Anything.Whatever").AsOsAgnostic();
            new LocalMovie { Path = path }.IsImmutableSubdirectory.Should().BeTrue();
        }

        [Test]
        public void should_indicate_not_immutable_directory_for_regular_files()
        {
            string path = @"C:\Movies\Movie Name (2000)\Movie Name (2000).mkv".AsOsAgnostic();
            new LocalMovie { Path = path }.IsImmutableSubdirectory.Should().BeFalse();

            path = @"C:\Movies\Movie Name (2000)\VTS_01_0.VOB".AsOsAgnostic();
            new LocalMovie { Path = path }.IsImmutableSubdirectory.Should().BeFalse();
        }
    }
}
