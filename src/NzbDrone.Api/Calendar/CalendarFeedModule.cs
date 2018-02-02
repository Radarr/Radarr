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
using NzbDrone.Core.Tv;
using Nancy.Responses;
using NzbDrone.Core.Tags;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Api.Calendar
{
    public class CalendarFeedModule : NzbDroneFeedModule
    {
        private readonly IMovieService _movieService;
        private readonly ITagService _tagService;

        public CalendarFeedModule(IMovieService movieService, ITagService tagService)
            : base("calendar")
        {
            _movieService = movieService;
            _tagService = tagService;

            Get["/NzbDrone.ics"] = options => GetCalendarFeed();
            Get["/Sonarr.ics"] = options => GetCalendarFeed();
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

                var occurrence = calendar.Create<Event>();
                occurrence.Uid = "NzbDrone_movie_" + movie.Id;
                occurrence.Status = movie.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;

                switch (movie.Status)
                {
                    case MovieStatusType.PreDB:
                        if (movie.PhysicalRelease != null)
                        {
                            occurrence.Start = new CalDateTime(movie.PhysicalRelease.Value) { HasTime = true };
                            occurrence.End = new CalDateTime(movie.PhysicalRelease.Value.AddMinutes(movie.Runtime)) { HasTime = true };
                        }
                        break;

                    case MovieStatusType.InCinemas:
                        if (movie.InCinemas != null)
                        {
                            occurrence.Start = new CalDateTime(movie.InCinemas.Value) { HasTime = true };
                            occurrence.End = new CalDateTime(movie.InCinemas.Value.AddMinutes(movie.Runtime)) { HasTime = true };
                        }
                        break;

                    case MovieStatusType.Announced:
                        continue; // no date

                    default:
                        if (movie.PhysicalRelease != null)
                        {
                            occurrence.Start = new CalDateTime(movie.PhysicalRelease.Value) { HasTime = true };
                            occurrence.End = new CalDateTime(movie.PhysicalRelease.Value.AddMinutes(movie.Runtime)) { HasTime = true };
                        }
                        break;
                }

                occurrence.Description = movie.Overview;
                occurrence.Categories = new List<string>() { movie.Studio };

                occurrence.Summary = $"{movie.Title}";

            }

            var serializer = (IStringSerializer) new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return new TextResponse(icalendar, "text/calendar");
        }
    }
}
