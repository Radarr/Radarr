using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class CleanCollectionTheFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                    .CreateNew()
                    .With(e => e.Title = "Movie Title")
                    .Build();

            _movieFile = new MovieFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RadarrTest" };

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameMovies = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                .Setup(v => v.All())
                .Returns(new List<CustomFormat>());
        }

        [TestCase("The Badger's Collection", "Badgers Collection, The")]
        [TestCase("@ The Movies Collection", "@ The Movies Collection")] // This doesn't seem right; see: FileNameBuilder.ScenifyRemoveChars, looks like it has the "at sign" in the regex
        [TestCase("A Stupid/Idiotic Collection", "Stupid Idiotic Collection, A")]
        [TestCase("An Astounding & Amazing Collection", "Astounding and Amazing Collection, An")]
        [TestCase("The Amazing Animal-Hero's Collection (2001)", "Amazing Animal-Heros Collection, The 2001")]
        [TestCase("A Different Movië (AU)", "Different Movie, A AU")]
        [TestCase("The Repairër (ZH) (2015)", "Repairer, The ZH 2015")]
        [TestCase("The Eighth Sensë 2 (Thai)", "Eighth Sense 2, The Thai")]
        [TestCase("The Astonishing Jæg (Latin America)", "Astonishing Jaeg, The Latin America")]
        [TestCase("The Hampster Pack (B&F)", "Hampster Pack, The BandF")]
        [TestCase("The Gásm: I (Almost) Got Away With It (1900)", "Gasm I Almost Got Away With It, The 1900")]
        [TestCase(null, "")]
        public void should_get_expected_title_back(string collection, string expected)
        {
            SetCollectionName(_movie, collection);
            _namingConfig.StandardMovieFormat = "{Movie CleanCollectionThe}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expected);
        }

        [TestCase("A")]
        [TestCase("Anne")]
        [TestCase("Theodore")]
        [TestCase("3%")]
        public void should_not_change_title(string collection)
        {
            SetCollectionName(_movie, collection);
            _namingConfig.StandardMovieFormat = "{Movie CleanCollectionThe}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(collection);
        }

        private void SetCollectionName(Movie movie, string collectionName)
        {
            var metadata = new MovieMetadata()
            {
                CollectionTitle = collectionName,
            };
            movie.MovieMetadata = new Core.Datastore.LazyLoaded<MovieMetadata>(metadata);
            movie.MovieMetadata.Value.CollectionTitle = collectionName;
        }
    }
}
