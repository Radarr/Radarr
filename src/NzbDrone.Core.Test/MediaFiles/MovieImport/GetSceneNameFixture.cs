using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport
{
    [TestFixture]
    public class GetSceneNameFixture : CoreTest
    {
        private LocalMovie _localMovie;
        private string _movieName = "movie.title.2022.dvdrip.x264-ingot";

        [SetUp]
        public void Setup()
        {
            var movie = Builder<Movie>.CreateNew()
                                        .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                        .With(s => s.Path = @"C:\Test\Movies\Movie Title".AsOsAgnostic())
                                        .Build();

            _localMovie = new LocalMovie
            {
                Movie = movie,
                Path = Path.Combine(movie.Path, "Movie Title - 2022 - Episode Title.mkv"),
                Quality = new QualityModel(Quality.Bluray720p),
                ReleaseGroup = "DRONE"
            };
        }

        private void GivenExistingFileOnDisk()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(new List<MovieFile>());
        }

        [Test]
        public void should_use_download_client_item_title_as_scene_name()
        {
            _localMovie.DownloadClientMovieInfo = new ParsedMovieInfo
            {
                ReleaseTitle = _movieName
            };

            SceneNameCalculator.GetSceneName(_localMovie).Should()
                               .Be(_movieName);
        }

        [Test]
        public void should_not_use_download_client_item_title_as_scene_name_if_there_are_other_video_files()
        {
            _localMovie.OtherVideoFiles = true;
            _localMovie.DownloadClientMovieInfo = new ParsedMovieInfo
            {
                ReleaseTitle = _movieName
            };

            _localMovie.Path = Path.Combine(@"C:\Test\Unsorted Movies", _movieName)
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localMovie).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_file_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localMovie.Path = Path.Combine(@"C:\Test\Unsorted Movies", _movieName + ".mkv")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localMovie).Should()
                               .Be(_movieName);
        }

        [Test]
        public void should_not_use_file_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localMovie.Path = Path.Combine(@"C:\Test\Unsorted Movies", _movieName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localMovie).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_folder_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localMovie.FolderMovieInfo = new ParsedMovieInfo
            {
                ReleaseTitle = _movieName
            };

            SceneNameCalculator.GetSceneName(_localMovie).Should()
                               .Be(_movieName);
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localMovie.Path = Path.Combine(@"C:\Test\Unsorted Movies", _movieName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localMovie.FolderMovieInfo = new ParsedMovieInfo
            {
                ReleaseTitle = "aaaaa"
            };

            SceneNameCalculator.GetSceneName(_localMovie).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_there_are_other_video_files()
        {
            _localMovie.OtherVideoFiles = true;
            _localMovie.Path = Path.Combine(@"C:\Test\Unsorted Movies", _movieName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localMovie.FolderMovieInfo = new ParsedMovieInfo
            {
                ReleaseTitle = _movieName
            };

            SceneNameCalculator.GetSceneName(_localMovie).Should()
                               .BeNull();
        }

        [TestCase(".mkv")]
        [TestCase(".par2")]
        [TestCase(".nzb")]
        public void should_remove_extension_from_nzb_title_for_scene_name(string extension)
        {
            _localMovie.DownloadClientMovieInfo = new ParsedMovieInfo
            {
                ReleaseTitle = _movieName + extension
            };

            SceneNameCalculator.GetSceneName(_localMovie).Should()
                               .Be(_movieName);
        }
    }
}
