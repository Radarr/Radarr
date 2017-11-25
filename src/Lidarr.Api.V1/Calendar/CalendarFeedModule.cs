using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Factory;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Music;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.Calendar
{
    public class CalendarFeedModule : LidarrV1FeedModule
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

            Get["/Lidarr.ics"] = options => GetCalendarFeed();
        }

        private Response GetCalendarFeed()
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
                ProductId = "-//lidarr.audio//Lidarr//EN"
            };

            var calendarName = "Lidarr Music Schedule";
            calendar.AddProperty(new CalendarProperty("NAME", calendarName));
            calendar.AddProperty(new CalendarProperty("X-WR-CALNAME", calendarName));

            foreach (var album in albums.OrderBy(v => v.ReleaseDate.Value))
            {
                var artist = _artistService.GetArtist(album.ArtistId); // Temp fix TODO: Figure out why Album.Artist is not populated during AlbumsBetweenDates Query

                if (tags.Any() && tags.None(artist.Tags.Contains))
                {
                    continue;
                }

                var occurrence = calendar.Create<Event>();
                occurrence.Uid = "NzbDrone_album_" + album.Id;
                //occurrence.Status = album.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
                //occurrence.Description = album.Overview;
                //occurrence.Categories = new List<string>() { album.Series.Network };

                occurrence.Start = new CalDateTime(album.ReleaseDate.Value.ToLocalTime()) { HasTime = false };

                occurrence.Summary = $"{artist.Name} - {album.Title}";
            }

            var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return new TextResponse(icalendar, "text/calendar");
        }
    }
}
