using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Test.MovieTests.MovieServiceTests
{
	[TestFixture]
	public class AddMovieFixture : CoreTest<MovieService>
	{
		private Movie fakeMovie;

		[SetUp]
		public void Setup()
		{
			fakeMovie = Builder<Movie>.CreateNew().Build();
		}

		[Test]
		public void movie_added_event_should_have_proper_path()
		{
			fakeMovie.Path = null;
			fakeMovie.RootFolderPath = @"C:\Test\Movies";

			Mocker.GetMock<IBuildFileNames>()
				  .Setup(s => s.GetMovieFolder(fakeMovie, null))
				  .Returns(fakeMovie.Title);

			var series = Subject.AddMovie(fakeMovie);

			series.Path.Should().NotBeNull();

		}

	}
}
