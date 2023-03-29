using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Others
{
    [TestFixture]
    public class OtherExtraServiceFixture : CoreTest<OtherExtraService>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private LocalMovie _localMovie;

        private string _movieFolder;
        private string _releaseFolder;

        [SetUp]
        public void Setup()
        {
            _movieFolder = @"C:\Test\Movies\Movie Title".AsOsAgnostic();
            _releaseFolder = @"C:\Test\Unsorted Movies\Movie.Title.2022".AsOsAgnostic();

            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = _movieFolder)
                                     .Build();

            _movieFile = Builder<MovieFile>.CreateNew()
                                               .With(f => f.Path = Path.Combine(_movie.Path, "Movie Title - 2022.mkv").AsOsAgnostic())
                                               .With(f => f.RelativePath = @"Movie Title - 2022.mkv")
                                               .Build();

            _localMovie = Builder<LocalMovie>.CreateNew()
                                                 .With(l => l.Movie = _movie)
                                                 .With(l => l.Path = Path.Combine(_releaseFolder, "Movie.Title.2022.mkv").AsOsAgnostic())
                                                 .With(l => l.FileMovieInfo = new ParsedMovieInfo
                                                 {
                                                     MovieTitles = new List<string> { "Movie Title" },
                                                     Year = 2022
                                                 })
                                                 .Build();
        }

        [Test]
        [TestCase("Movie Title - 2022.nfo", "Movie Title - 2022.nfo")]
        [TestCase("Movie.Title.2022.nfo", "Movie Title - 2022.nfo")]
        [TestCase("Movie Title 2022.nfo", "Movie Title - 2022.nfo")]
        [TestCase("Movie_Title_2022.nfo", "Movie Title - 2022.nfo")]
        [TestCase(@"Movie.Title.2022\thumb.jpg", "Movie Title - 2022.jpg")]
        public void should_import_matching_file(string filePath, string expectedOutputPath)
        {
            var files = new List<string> { Path.Combine(_releaseFolder, filePath).AsOsAgnostic() };

            var results = Subject.ImportFiles(_localMovie, _movieFile, files, true).ToList();

            results.Count.Should().Be(1);

            results[0].RelativePath.AsOsAgnostic().PathEquals(expectedOutputPath.AsOsAgnostic()).Should().Be(true);
        }

        [Test]
        public void should_not_import_multiple_nfo_files()
        {
            var files = new List<string>
            {
                Path.Combine(_releaseFolder, "Movie.Title.2022.nfo").AsOsAgnostic(),
                Path.Combine(_releaseFolder, "Movie_Title_2022.nfo").AsOsAgnostic(),
            };

            var results = Subject.ImportFiles(_localMovie, _movieFile, files, true).ToList();

            results.Count.Should().Be(1);
        }
    }
}
