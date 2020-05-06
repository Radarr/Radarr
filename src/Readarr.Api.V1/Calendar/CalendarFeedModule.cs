using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;
using NzbDrone.Core.Tags;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Calendar
{
    public class CalendarFeedModule : ReadarrV1FeedModule
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly ITagService _tagService;

        public CalendarFeedModule(IAlbumService albumService, IArtistService artistService, ITagService tagService)
            : base("calendar")
        {
            _albumService = albumService;
            _artistService = artistService;
            _tagService = tagService;

            Get("/Readarr.ics", options => GetCalendarFeed());
        }

        private object GetCalendarFeed()
        {
            var pastDays = 7;
            var futureDays = 28;
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var unmonitored = Request.GetBooleanQueryParameter("unmonitored");
            var tags = new List<int>();

            var queryPastDays = Request.Query.PastDays;
            var queryFutureDays = Request.Query.FutureDays;
            var queryTags = Request.Query.Tags;

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

            if (queryTags.HasValue)
            {
                var tagInput = (string)queryTags.Value.ToString();
                tags.AddRange(tagInput.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            var albums = _albumService.AlbumsBetweenDates(start, end, unmonitored);
            var calendar = new Ical.Net.Calendar
            {
                ProductId = "-//readarr.com//Readarr//EN"
            };

            var calendarName = "Readarr Music Schedule";
            calendar.AddProperty(new CalendarProperty("NAME", calendarName));
            calendar.AddProperty(new CalendarProperty("X-WR-CALNAME", calendarName));

            foreach (var album in albums.OrderBy(v => v.ReleaseDate.Value))
            {
                var artist = _artistService.GetArtist(album.AuthorId); // Temp fix TODO: Figure out why Album.Artist is not populated during AlbumsBetweenDates Query

                if (tags.Any() && tags.None(artist.Tags.Contains))
                {
                    continue;
                }

                var occurrence = calendar.Create<CalendarEvent>();
                occurrence.Uid = "Readarr_album_" + album.Id;

                //occurrence.Status = album.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
                occurrence.Description = album.Overview;
                occurrence.Categories = album.Genres;

                occurrence.Start = new CalDateTime(album.ReleaseDate.Value.ToLocalTime()) { HasTime = false };

                occurrence.Summary = $"{artist.Name} - {album.Title}";
            }

            var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return new TextResponse(icalendar, "text/calendar");
        }
    }
}
