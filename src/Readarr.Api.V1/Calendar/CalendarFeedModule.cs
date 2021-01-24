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
using NzbDrone.Core.Books;
using NzbDrone.Core.Tags;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Calendar
{
    public class CalendarFeedModule : ReadarrV1FeedModule
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly ITagService _tagService;

        public CalendarFeedModule(IBookService bookService, IAuthorService authorService, ITagService tagService)
            : base("calendar")
        {
            _bookService = bookService;
            _authorService = authorService;
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

            var books = _bookService.BooksBetweenDates(start, end, unmonitored);
            var calendar = new Ical.Net.Calendar
            {
                ProductId = "-//readarr.com//Readarr//EN"
            };

            var calendarName = "Readarr Book Schedule";
            calendar.AddProperty(new CalendarProperty("NAME", calendarName));
            calendar.AddProperty(new CalendarProperty("X-WR-CALNAME", calendarName));

            foreach (var book in books.OrderBy(v => v.ReleaseDate.Value))
            {
                var author = _authorService.GetAuthor(book.AuthorId); // Temp fix TODO: Figure out why Book.Author is not populated during BooksBetweenDates Query

                if (tags.Any() && tags.None(author.Tags.Contains))
                {
                    continue;
                }

                var occurrence = calendar.Create<CalendarEvent>();
                occurrence.Uid = "Readarr_book_" + book.Id;

                //occurrence.Status = book.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
                occurrence.Description = book.Editions.Value.Single(x => x.Monitored).Overview;
                occurrence.Categories = book.Genres;

                occurrence.Start = new CalDateTime(book.ReleaseDate.Value.ToLocalTime()) { HasTime = false };

                occurrence.Summary = $"{author.Name} - {book.Title}";
            }

            var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return new TextResponse(icalendar, "text/calendar");
        }
    }
}
