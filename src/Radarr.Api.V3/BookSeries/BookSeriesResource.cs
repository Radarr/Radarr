using System.Collections.Generic;
using System.Linq;
using Radarr.Http.REST;
using BookSeriesModel = NzbDrone.Core.BookSeries.BookSeries;

namespace Radarr.Api.V3.BookSeries
{
    public class BookSeriesResource : RestResource
    {
        public BookSeriesResource()
        {
            Monitored = true;
        }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Description { get; set; }
        public string ForeignSeriesId { get; set; }
        public int? AuthorId { get; set; }
        public bool Monitored { get; set; }
    }

    public static class BookSeriesResourceMapper
    {
        public static BookSeriesResource ToResource(this BookSeriesModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new BookSeriesResource
            {
                Id = model.Id,
                Title = model.Title,
                SortTitle = model.SortTitle,
                Description = model.Description,
                ForeignSeriesId = model.ForeignSeriesId,
                AuthorId = model.AuthorId,
                Monitored = model.Monitored
            };
        }

        public static BookSeriesModel ToModel(this BookSeriesResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new BookSeriesModel
            {
                Id = resource.Id,
                Title = resource.Title,
                SortTitle = resource.SortTitle,
                Description = resource.Description,
                ForeignSeriesId = resource.ForeignSeriesId,
                AuthorId = resource.AuthorId,
                Monitored = resource.Monitored
            };
        }

        public static BookSeriesModel ToModel(this BookSeriesResource resource, BookSeriesModel bookSeries)
        {
            var updatedBookSeries = resource.ToModel();

            bookSeries.Title = updatedBookSeries.Title;
            bookSeries.SortTitle = updatedBookSeries.SortTitle;
            bookSeries.Description = updatedBookSeries.Description;
            bookSeries.ForeignSeriesId = updatedBookSeries.ForeignSeriesId;
            bookSeries.AuthorId = updatedBookSeries.AuthorId;
            bookSeries.Monitored = updatedBookSeries.Monitored;

            return bookSeries;
        }

        public static List<BookSeriesResource> ToResource(this IEnumerable<BookSeriesModel> bookSeriesList)
        {
            return bookSeriesList.Select(ToResource).ToList();
        }

        public static List<BookSeriesModel> ToModel(this IEnumerable<BookSeriesResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
