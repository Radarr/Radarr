using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Factory;
using NzbDrone.Core.Movies;
using Nancy.Responses;
using NzbDrone.Core.Tags;
using NzbDrone.Common.Extensions;

namespace Radarr.Api.V2.Calendar
{
    public class CalendarFeedModule : RadarrV2FeedModule
    {
        private readonly IMovieService _movieService;
        private readonly ITagService _tagService;

        public CalendarFeedModule(IMovieService movieService, ITagService tagService)
            : base("calendar")
        {
            _movieService = movieService;
            _tagService = tagService;
            
            Get["/Radarr.ics"] = options => GetCalendarFeed();
        }

        private Response GetCalendarFeed()
        {
            var pastDays = 7;
            var futureDays = 28;
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var unmonitored = false;
            //var premiersOnly = false;
            var tags = new List<int>();

            // TODO: Remove start/end parameters in v3, they don't work well for iCal
            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;
            var queryPastDays = Request.Query.PastDays;
            var queryFutureDays = Request.Query.FutureDays;
            var queryUnmonitored = Request.Query.Unmonitored;
            // var queryPremiersOnly = Request.Query.PremiersOnly;
            var queryTags = Request.Query.Tags;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            if (queryPastDays.HasValue)
            {
                pastDays = int.Parse(queryPastDays.Value);
                start = DateTime.Today.AddDays(-pastDays);
            }

            if (queryFutureDays.HasValue)
            {
                futureDays = int.Parse(queryFutureDays.Value);
                end = DateTime.Today.AddDays(futureDays);
            }

            if (queryUnmonitored.HasValue)
            {
                unmonitored = bool.Parse(queryUnmonitored.Value);
            }

            //if (queryPremiersOnly.HasValue)
            //{
            //    premiersOnly = bool.Parse(queryPremiersOnly.Value);
            //}

            if (queryTags.HasValue)
            {
                var tagInput = (string)queryTags.Value.ToString();
                tags.AddRange(tagInput.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
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

                CreateEvent(calendar, movie, true);
                CreateEvent(calendar, movie, false);

            }

            var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return new TextResponse(icalendar, "text/calendar");
        }

        private void CreateEvent(Ical.Net.Calendar calendar, Movie movie, bool cinemasRelease)
        {
            var date = cinemasRelease ? movie.InCinemas : movie.PhysicalRelease;
            if (!date.HasValue)
            {
                return;
            }

            var occurrence = calendar.Create<Event>();
            occurrence.Uid = "NzbDrone_movie_" + movie.Id + (cinemasRelease ? "_cinemas" : "_physical");
            occurrence.Status = movie.Status == MovieStatusType.Announced ? EventStatus.Tentative : EventStatus.Confirmed;

            occurrence.Start = new CalDateTime(date.Value);
            occurrence.End = occurrence.Start;
            occurrence.IsAllDay = true;

            occurrence.Description = movie.Overview;
            occurrence.Categories = new List<string>() { movie.Studio };

            var physicalText = movie.PhysicalReleaseNote.IsNotNullOrWhiteSpace()
                ? $"(Physical Release, {movie.PhysicalReleaseNote})"
                : "(Physical Release)";
            occurrence.Summary = $"{movie.Title} " + (cinemasRelease ? "(Theatrical Release)" : physicalText);
        }
    }
}
