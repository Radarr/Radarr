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
            int pastDays = 7,
            int futureDays = 28,
            string tagList = "",
            bool unmonitored = false,
            bool hideMinAvailabilityUnmet = false)
        {
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var tags = new List<int>();

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

                if (!hideMinAvailabilityUnmet || movie.MinimumAvailability != MovieStatusType.Released)
                {
                    CreateEvent(calendar, movie.MovieMetadata, "cinematic");
                }

                CreateEvent(calendar, movie.MovieMetadata, "digital");
                CreateEvent(calendar, movie.MovieMetadata, "physical");
            }

            var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return Content(icalendar, "text/calendar");
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
                "cinematic" => (movie.InCinemas, "_cinemas", "(Theatrical Release)"),
                "digital" => (movie.DigitalRelease, "_digital", "(Digital Release)"),
                "physical" => (movie.PhysicalRelease, "_physical", "(Physical Release)"),
                _ => default
            };
    }
}
