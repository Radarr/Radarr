using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Integration.Test.Client;
using Radarr.Api.V3.Calendar;
using Radarr.Api.V3.Movies;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class CalendarFixture : IntegrationTest
    {
        public ClientBase<MovieResource> Calendar;

        private static string[] ReleaseTypes =
        {
            ReleaseType.Digital, ReleaseType.Physical, ReleaseType.InCinemas, null
        };

        protected override void InitRestClients()
        {
            base.InitRestClients();

            Calendar = new ClientBase<MovieResource>(RestClient, ApiKey, "calendar");
        }

        [Test]
        public void should_be_able_to_get_movies()
        {
            var movie = EnsureMovie(680, "Pulp Fiction", true);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(1993, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(1995, 10, 3).ToString("s") + "Z");
            var items = Calendar.Get<List<MovieResource>>(request);

            items = items.Where(v => v.Id == movie.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("Pulp Fiction");
        }

        [Test]
        public void should_not_be_able_to_get_unmonitored_movies()
        {
            var movie = EnsureMovie(680, "Pulp Fiction", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(1993, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(1995, 10, 3).ToString("s") + "Z");
            request.AddParameter("unmonitored", "false");
            var items = Calendar.Get<List<MovieResource>>(request);

            items = items.Where(v => v.Id == movie.Id).ToList();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_get_unmonitored_movies()
        {
            var movie = EnsureMovie(680, "Pulp Fiction", false);

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(1993, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(1995, 10, 3).ToString("s") + "Z");
            request.AddParameter("unmonitored", "true");
            var items = Calendar.Get<List<MovieResource>>(request);

            items = items.Where(v => v.Id == movie.Id).ToList();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("Pulp Fiction");
        }

        [Test]
        [Pairwise]
        public void with_releaseTypes_filter_should_filter_by_release_type(
            [ValueSource(nameof(ReleaseTypes))] string first,
            [ValueSource(nameof(ReleaseTypes))] string second,
            [ValueSource(nameof(ReleaseTypes))] string third)
        {
            var movie = EnsureMovie(680, "Pulp Fiction", true, movie =>
            {
                movie.MinimumAvailability = Core.Movies.MovieStatusType.Released;
            });

            var releaseTypes = new[] { first, second, third }.Where(item => item != null).ToList();

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(1993, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(1995, 10, 3).ToString("s") + "Z");
            request.AddParameter("releaseTypes[]", string.Join(',', releaseTypes));
            var items = Calendar.Get<List<MovieResource>>(request);

            items = items.Where(v => v.Id == movie.Id).ToList();

            if (!releaseTypes.Any())
            {
                items.First().DigitalRelease.Should().NotBeNull();
                items.First().PhysicalRelease.Should().NotBeNull();
                items.First().InCinemas.Should().NotBeNull();
                return;
            }

            if (releaseTypes.Contains(ReleaseType.Digital))
            {
                items.First().DigitalRelease.Should().NotBeNull();
            }

            if (releaseTypes.Contains(ReleaseType.InCinemas))
            {
                items.First().InCinemas.Should().NotBeNull();
            }

            if (releaseTypes.Contains(ReleaseType.Physical))
            {
                items.First().PhysicalRelease.Should().NotBeNull();
            }
        }

        [Test]
        public void with_meetsMinimumAvailability_filter_and_when_minimum_availability_met_should_get_cinematic_release_date()
        {
            var movie = EnsureMovie(680, "Pulp Fiction", true, movie =>
            {
                movie.MinimumAvailability = Core.Movies.MovieStatusType.InCinemas;
            });

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(1993, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(1995, 10, 3).ToString("s") + "Z");
            request.AddParameter("releaseType[]", ReleaseType.MeetsMinimumAvailability);
            var items = Calendar.Get<List<MovieResource>>(request);

            items = items.Where(v => v.Id == movie.Id).ToList();

            items.First().InCinemas.Should().NotBeNull();
        }

        [Test]
        public void with_meetsMinimumAvailability_filter_and_when_minimum_availability_unmet_should_not_get_cinematic_release_date()
        {
            var movie = EnsureMovie(680, "Pulp Fiction", true, movie =>
            {
                movie.MinimumAvailability = Core.Movies.MovieStatusType.Released;
            });

            var request = Calendar.BuildRequest();
            request.AddParameter("start", new DateTime(1993, 10, 1).ToString("s") + "Z");
            request.AddParameter("end", new DateTime(1995, 10, 3).ToString("s") + "Z");
            request.AddParameter("releaseType[]", ReleaseType.MeetsMinimumAvailability);
            var items = Calendar.Get<List<MovieResource>>(request);

            items = items.Where(v => v.Id == movie.Id).ToList();

            items.First().InCinemas.Should().BeNull();
        }

        public override MovieResource EnsureMovie(int tmdbid, string movieTitle, bool? monitored = null, Action<MovieResource> options = null)
        {
            var movie = Movies.All().FirstOrDefault(v => v.TmdbId == tmdbid);
            if (movie != null)
            {
                Movies.Delete(movie.Id);
            }

            return base.EnsureMovie(tmdbid, movieTitle, monitored, options);
        }
    }
}
