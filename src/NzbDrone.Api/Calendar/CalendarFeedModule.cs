using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Factory;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;
using Nancy.Responses;
using NzbDrone.Core.Tags;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Api.Calendar
{
    public class CalendarFeedModule : NzbDroneFeedModule
    {
        private readonly IAlbumService _albumService;
        private readonly ITagService _tagService;

        public CalendarFeedModule(IAlbumService albumService, ITagService tagService)
            : base("calendar")
        {
            _albumService = albumService;
            _tagService = tagService;

            Get["/NzbDrone.ics"] = options => GetCalendarFeed();
            Get["/Lidarr.ics"] = options => GetCalendarFeed();
        }

        private Response GetCalendarFeed()
        {
            var pastDays = 7;
            var futureDays = 28;
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var unmonitored = false;
            var premiersOnly = false;
            var asAllDay = false;
            var tags = new List<int>();

            // TODO: Remove start/end parameters in v3, they don't work well for iCal
            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;
            var queryPastDays = Request.Query.PastDays;
            var queryFutureDays = Request.Query.FutureDays;
            var queryUnmonitored = Request.Query.Unmonitored;
            var queryPremiersOnly = Request.Query.PremiersOnly;
            var queryAsAllDay = Request.Query.AsAllDay;
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

            if (queryPremiersOnly.HasValue)
            {
                premiersOnly = bool.Parse(queryPremiersOnly.Value);
            }

            if (queryAsAllDay.HasValue)
            {
                asAllDay = bool.Parse(queryAsAllDay.Value);
            }

            if (queryTags.HasValue)
            {
                var tagInput = (string)queryTags.Value.ToString();
                tags.AddRange(tagInput.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            var albums = _albumService.AlbumsBetweenDates(start, end, unmonitored);
            var calendar = new Ical.Net.Calendar
            {
                // This will need to point to the hosted web site
                // TODO
                ProductId = "-//lidarr.audio//Lidarr//EN"
            };



            foreach (var album in albums.OrderBy(v => v.ReleaseDate))
            {
                //if (premiersOnly && (album.SeasonNumber == 0 || album.EpisodeNumber != 1))
                //{
                //    continue;
                //}

                if (tags.Any() && tags.None(album.Artist.Tags.Contains))
                {
                    continue;
                }

                var occurrence = calendar.Create<Event>();
                occurrence.Uid = "NzbDrone_album_" + album.Id;
                //occurrence.Status = album.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
                //occurrence.Description = album.Overview;
                //occurrence.Categories = new List<string>() { album.Artist. };

                occurrence.Start = new CalDateTime(album.ReleaseDate.Value) { HasTime = false };
                
                occurrence.Summary =$"{album.Artist.Name} - {album.Title}";
                
            }

            var serializer = (IStringSerializer) new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return new TextResponse(icalendar, "text/calendar");
        }
    }
}
