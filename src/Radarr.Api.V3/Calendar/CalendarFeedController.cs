using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tags;
using Radarr.Http;

namespace Radarr.Api.V3.Calendar
{
    [V3FeedController("calendar")]
    public class CalendarFeedController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly ITagService _tagService;

        public CalendarFeedController(IMovieService movieService, ITagService tagService)
        {
            _movieService = movieService;
            _tagService = tagService;
        }

        [HttpGet("Radarr.ics")]
        public IActionResult GetCalendarFeed(
            string[] releaseTypes,
            int pastDays = 7,
            int futureDays = 28,
            string tagList = "",
            bool unmonitored = false,
            bool hideMinAvailabilityUnmet = false)
        {
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var tags = new List<int>();
            var filteredReleaseTypes = releaseTypes
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();
            if (filteredReleaseTypes.Length == 0)
            {
                filteredReleaseTypes = new[] { ReleaseType.InCinemas, ReleaseType.Digital, ReleaseType.Physical };
            }

            if (tagList.IsNotNullOrWhiteSpace())
            {
                tags.AddRange(tagList.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            var movies = _movieService.GetMoviesBetweenDates(start, end, unmonitored);
            var calendar = new Ical.Net.Calendar
            {
                ProductId = "-//radarr.video//Radarr//EN"
            };

            var calendarName = "Radarr Movies Calendar";
            calendar.AddProperty(new CalendarProperty("NAME", calendarName));
            calendar.AddProperty(new CalendarProperty("X-WR-CALNAME", calendarName));

            foreach (var movie in movies.OrderBy(v => v.Added))
            {
                if (tags.Any() && tags.None(movie.Tags.Contains))
                {
                    continue;
                }

                CreateEventsByReleaseType(calendar, movie, filteredReleaseTypes);
            }

            var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return Content(icalendar, "text/calendar");
        }

        private void CreateEventsByReleaseType(Ical.Net.Calendar calendar, Movie movie, string[] releaseTypes)
        {
            var meetsMinimumAvailability = releaseTypes.Contains(ReleaseType.MeetsMinimumAvailability);
            if (meetsMinimumAvailability && movie.MinimumAvailability == MovieStatusType.Released)
            {
                CreateEvent(calendar, movie.MovieMetadata, ReleaseType.Digital);
                CreateEvent(calendar, movie.MovieMetadata, ReleaseType.Physical);
            }
            else if (meetsMinimumAvailability || releaseTypes.Empty())
            {
                CreateEvent(calendar, movie.MovieMetadata, ReleaseType.InCinemas);
                CreateEvent(calendar, movie.MovieMetadata, ReleaseType.Digital);
                CreateEvent(calendar, movie.MovieMetadata, ReleaseType.Physical);
            }
            else
            {
                foreach (var releaseType in releaseTypes)
                {
                    CreateEvent(calendar, movie.MovieMetadata, releaseType);
                }
            }
        }

        private void CreateEvent(Ical.Net.Calendar calendar, MovieMetadata movie, string releaseType)
        {
            var (date, eventType, summaryText) = GetEventInfo(movie, releaseType);

            if (!date.HasValue)
            {
                return;
            }

            var occurrence = calendar.Create<CalendarEvent>();
            occurrence.Uid = "Radarr_movie_" + movie.Id + eventType;
            occurrence.Status = movie.Status == MovieStatusType.Announced ? EventStatus.Tentative : EventStatus.Confirmed;

            occurrence.Start = new CalDateTime(date.Value);
            occurrence.End = occurrence.Start;
            occurrence.IsAllDay = true;

            occurrence.Description = movie.Overview;
            occurrence.Categories = new List<string>() { movie.Studio };

            occurrence.Summary = $"{movie.Title} " + summaryText;
        }

        private (DateTime? date, string eventType, string summaryText) GetEventInfo(MovieMetadata movie, string releaseType)
            => releaseType switch
            {
                ReleaseType.InCinemas => (movie.InCinemas, "_cinemas", "(Theatrical Release)"),
                ReleaseType.Digital => (movie.DigitalRelease, "_digital", "(Digital Release)"),
                ReleaseType.Physical => (movie.PhysicalRelease, "_physical", "(Physical Release)"),
                _ => default
            };
    }
}
