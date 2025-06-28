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
    public class CollectionTheFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                    .CreateNew()
                    .With(e => e.Title = "Batman")
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

        [TestCase("The Wolverine Collection", "Wolverine Collection, The")]
        [TestCase("The Transporter Collection", "Transporter Collection, The")]
        [TestCase("A Stupid Collection", "Stupid Collection, A")]
        [TestCase("An Inconvenient Collection", "Inconvenient Collection, An")]
        [TestCase("The Amazing Spider-Man Collection (Garfield)", "Amazing Spider-Man Collection, The (Garfield)")]
        [TestCase("A League Of Their Own (AU)", "League Of Their Own, A (AU)")]
        [TestCase("The Fixer (ZH) (2015)", "Fixer, The (ZH) (2015)")]
        [TestCase("The Sixth Sense 2 (Thai)", "Sixth Sense 2, The (Thai)")]
        [TestCase("The Amazing Race (Latin America)", "Amazing Race, The (Latin America)")]
        [TestCase("The Rat Pack (A&E)", "Rat Pack, The (A&E)")]
        [TestCase("The Climax: I (Almost) Got Away With It (2016)", "Climax - I (Almost) Got Away With It, The (2016)")]
        public void should_get_expected_title_back(string collection, string expected)
        {
            SetCollectionName(_movie, collection);
            _namingConfig.StandardMovieFormat = "{Movie CollectionThe}";

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
            _namingConfig.StandardMovieFormat = "{Movie CollectionThe}";

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
