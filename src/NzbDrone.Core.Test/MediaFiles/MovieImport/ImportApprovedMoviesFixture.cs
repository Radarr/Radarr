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

namespace NzbDrone.Core.Test.MediaFiles.MovieImport
{
    [TestFixture]

    //TODO: Update all of this for movies.
    public class ImportApprovedMoviesFixture : CoreTest<ImportApprovedMovie>
    {
        private List<ImportDecision> _rejectedDecisions;
        private List<ImportDecision> _approvedDecisions;

        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _rejectedDecisions = new List<ImportDecision>();
            _approvedDecisions = new List<ImportDecision>();

            var outputPath = @"C:\Test\Unsorted\TV\30.Rock.S01E01".AsOsAgnostic();

            var movie = Builder<Movie>.CreateNew()
                .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                .Build();

            _rejectedDecisions.Add(new ImportDecision(new LocalMovie(), new Rejection("Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalMovie(), new Rejection("Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalMovie(), new Rejection("Rejected!")));

            _approvedDecisions.Add(new ImportDecision(
                                       new LocalMovie
                                       {
                                           Movie = movie,
                                           Path = Path.Combine(movie.Path, "30 Rock - S01E01 - Pilot.avi"),
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

        private void GivenNewDownload()
        {
            _approvedDecisions.ForEach(a => a.LocalMovie.Path = Path.Combine(_downloadClientItem.OutputPath.ToString(), Path.GetFileName(a.LocalMovie.Path)));
        }

        private void GivenExistingFileOnDisk()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(new List<MovieFile>());
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
            GivenExistingFileOnDisk();

            Subject.Import(_approvedDecisions, false).Should().HaveCount(1);
        }

        [Test]
        public void should_only_import_approved()
        {
            GivenExistingFileOnDisk();

            var all = new List<ImportDecision>();
            all.AddRange(_rejectedDecisions);
            all.AddRange(_approvedDecisions);

            var result = Subject.Import(all, false);

            result.Should().HaveCount(all.Count);
            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_only_import_each_movie_once()
        {
            GivenExistingFileOnDisk();

            var all = new List<ImportDecision>();
            all.AddRange(_approvedDecisions);
            all.Add(new ImportDecision(_approvedDecisions.First().LocalMovie));

            var result = Subject.Import(all, false);

            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_move_new_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeMovieFile(It.IsAny<MovieFile>(), _approvedDecisions.First().LocalMovie, false),
                          Times.Once());
        }

        [Test]
        public void should_publish_MovieImportedEvent_for_new_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<MovieImportedEvent>()), Times.Once());
        }

        [Test]
        public void should_not_move_existing_files()
        {
            GivenExistingFileOnDisk();

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, false);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeMovieFile(It.IsAny<MovieFile>(), _approvedDecisions.First().LocalMovie, false),
                          Times.Never());
        }

        [Test]
        public void should_use_nzb_title_as_scene_name()
        {
            GivenNewDownload();
            _downloadClientItem.Title = "malcolm.in.the.middle.2015.dvdrip.xvid-ingot";

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.SceneName == _downloadClientItem.Title)));
        }

        [TestCase(".mkv")]
        [TestCase(".par2")]
        [TestCase(".nzb")]
        public void should_remove_extension_from_nzb_title_for_scene_name(string extension)
        {
            GivenNewDownload();
            var title = "malcolm.in.the.middle.2015.dvdrip.xvid-ingot";

            _downloadClientItem.Title = title + extension;

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.SceneName == title)));
        }

        [Test]
        public void should_use_file_name_as_scenename_only_if_it_looks_like_scenename()
        {
            GivenNewDownload();
            _approvedDecisions.First().LocalMovie.Path = Path.Combine(_downloadClientItem.OutputPath.ToString(), "movie.title.2018.dvdrip.xvid-ingot.mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.SceneName == "movie.title.2018.dvdrip.xvid-ingot")));
        }

        [Test]
        public void should_not_use_file_name_as_scenename_if_it_doesnt_looks_like_scenename()
        {
            GivenNewDownload();
            _approvedDecisions.First().LocalMovie.Path = Path.Combine(_downloadClientItem.OutputPath.ToString(), "aaaaa.mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.SceneName == null)));
        }

        [Test]
        public void should_use_folder_name_as_scenename_only_if_it_looks_like_scenename()
        {
            GivenNewDownload();
            _approvedDecisions.First().LocalMovie.Path = Path.Combine(_downloadClientItem.OutputPath.ToString(), "aaaaa.mkv");
            _approvedDecisions.First().LocalMovie.FolderMovieInfo = new ParsedMovieInfo
            {
                ReleaseTitle = "movie.title.2018.dvdrip.xvid-ingot"
            };

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.SceneName == "movie.title.2018.dvdrip.xvid-ingot")));
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_doesnt_looks_like_scenename()
        {
            GivenNewDownload();
            _approvedDecisions.First().LocalMovie.Path = Path.Combine(_downloadClientItem.OutputPath.ToString(), "aaaaa.mkv");
            _approvedDecisions.First().LocalMovie.FolderMovieInfo = new ParsedMovieInfo
            {
                ReleaseTitle = "aaaaa.mkv"
            };

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.SceneName == null)));
        }

        [Test]
        public void should_import_larger_files_first()
        {
            GivenExistingFileOnDisk();

            var fileDecision = _approvedDecisions.First();
            fileDecision.LocalMovie.Size = 1.Gigabytes();

            var sampleDecision = new ImportDecision(
                new LocalMovie
                {
                    Movie = fileDecision.LocalMovie.Movie,
                    Path = @"C:\Test\TV\30 Rock\30 Rock - 2017 - Pilot.avi".AsOsAgnostic(),
                    Quality = new QualityModel(),
                    Size = 80.Megabytes()
                });

            var all = new List<ImportDecision>();
            all.Add(fileDecision);
            all.Add(sampleDecision);

            var results = Subject.Import(all, false);

            results.Should().HaveCount(all.Count);
            results.Should().ContainSingle(d => d.Result == ImportResultType.Imported);
            results.Should().ContainSingle(d => d.Result == ImportResultType.Imported && d.ImportDecision.LocalMovie.Size == fileDecision.LocalMovie.Size);
        }

        [Test]
        public void should_copy_when_cannot_move_files_downloads()
        {
            GivenNewDownload();
            _downloadClientItem.Title = "30.Rock.S01E01";
            _downloadClientItem.CanMoveFiles = false;

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeMovieFile(It.IsAny<MovieFile>(), _approvedDecisions.First().LocalMovie, true), Times.Once());
        }

        [Test]
        public void should_use_override_importmode()
        {
            GivenNewDownload();
            _downloadClientItem.Title = "30.Rock.S01E01";
            _downloadClientItem.CanMoveFiles = false;

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem, ImportMode.Move);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeMovieFile(It.IsAny<MovieFile>(), _approvedDecisions.First().LocalMovie, false), Times.Once());
        }

        [Test]
        public void should_use_file_name_only_for_download_client_item_without_a_job_folder()
        {
            var fileName = "Series.Title.S01E01.720p.HDTV.x264-Sonarr.mkv";
            var path = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), fileName);

            _downloadClientItem.OutputPath = new OsPath(path);
            _approvedDecisions.First().LocalMovie.Path = path;

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.OriginalFilePath == fileName)));
        }

        [Test]
        public void should_use_folder_and_file_name_only_for_download_client_item_with_a_job_folder()
        {
            var name = "Series.Title.S01E01.720p.HDTV.x264-Sonarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), name);

            _downloadClientItem.OutputPath = new OsPath(outputPath);
            _approvedDecisions.First().LocalMovie.Path = Path.Combine(outputPath, name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.OriginalFilePath == $"{name}\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_include_intermediate_folders_for_download_client_item_with_a_job_folder()
        {
            var name = "Series.Title.S01E01.720p.HDTV.x264-Sonarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\TV\".AsOsAgnostic(), name);

            _downloadClientItem.OutputPath = new OsPath(outputPath);
            _approvedDecisions.First().LocalMovie.Path = Path.Combine(outputPath, "subfolder", name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.OriginalFilePath == $"{name}\\subfolder\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_use_folder_info_original_title_to_find_relative_path()
        {
            var name = "Transformers.2007.720p.BluRay.x264-Radarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\movies\".AsOsAgnostic(), name);
            var localMovie = _approvedDecisions.First().LocalMovie;

            localMovie.FolderMovieInfo = new ParsedMovieInfo { OriginalTitle = name };
            localMovie.Path = Path.Combine(outputPath, "subfolder", name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, null);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.OriginalFilePath == $"{name}\\subfolder\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_get_relative_path_when_there_is_no_grandparent_windows()
        {
            WindowsOnly();

            var name = "Transformers.2007.720p.BluRay.x264-Radarr";
            var outputPath = @"C:\".AsOsAgnostic();
            var localMovie = _approvedDecisions.First().LocalMovie;
            localMovie.FolderMovieInfo = new ParsedMovieInfo { ReleaseTitle = name };
            localMovie.Path = Path.Combine(outputPath, name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, null);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.OriginalFilePath == $"{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_get_relative_path_when_there_is_no_grandparent_for_UNC_path()
        {
            WindowsOnly();

            var name = "Transformers.2007.720p.BluRay.x264-Radarr";
            var outputPath = @"\\server\share";
            var localMovie = _approvedDecisions.First().LocalMovie;

            localMovie.FolderMovieInfo = new ParsedMovieInfo { ReleaseTitle = name };
            localMovie.Path = Path.Combine(outputPath, name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, null);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.OriginalFilePath == $"{name}.mkv")));
        }

        [Test]
        public void should_use_folder_info_original_title_to_find_relative_path_when_file_is_not_in_download_client_item_output_directory()
        {
            var name = "Transformers.2007.720p.BluRay.x264-Radarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\movie\".AsOsAgnostic(), name);
            var localMovie = _approvedDecisions.First().LocalMovie;

            _downloadClientItem.OutputPath = new OsPath(Path.Combine(@"C:\Test\Unsorted\movie-Other\".AsOsAgnostic(), name));
            localMovie.FolderMovieInfo = new ParsedMovieInfo { ReleaseTitle = name };
            localMovie.Path = Path.Combine(outputPath, "subfolder", name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.OriginalFilePath == $"subfolder\\{name}.mkv".AsOsAgnostic())));
        }

        [Test]
        public void should_use_folder_info_original_title_to_find_relative_path_when_download_client_item_has_an_empty_output_path()
        {
            var name = "Transformers.2007.720p.BluRay.x264-Radarr";
            var outputPath = Path.Combine(@"C:\Test\Unsorted\movies\".AsOsAgnostic(), name);
            var localMovie = _approvedDecisions.First().LocalMovie;

            localMovie.FolderMovieInfo = new ParsedMovieInfo { ReleaseTitle = name };
            localMovie.Path = Path.Combine(outputPath, "subfolder", name + ".mkv");

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<MovieFile>(c => c.OriginalFilePath == $"subfolder\\{name}.mkv".AsOsAgnostic())));
        }
    }
}
