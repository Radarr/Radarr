using System.Collections.Generic;
using System.Linq;
using Radarr.Http.REST;
using SeriesModel = NzbDrone.Core.Series.Series;

namespace Radarr.Api.V3.Series
{
    public class SeriesResource : RestResource
    {
        public SeriesResource()
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

    public static class SeriesResourceMapper
    {
        public static SeriesResource ToResource(this SeriesModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new SeriesResource
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

        public static SeriesModel ToModel(this SeriesResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new SeriesModel
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

        public static SeriesModel ToModel(this SeriesResource resource, SeriesModel series)
        {
            var updatedSeries = resource.ToModel();

            series.Title = updatedSeries.Title;
            series.SortTitle = updatedSeries.SortTitle;
            series.Description = updatedSeries.Description;
            series.ForeignSeriesId = updatedSeries.ForeignSeriesId;
            series.AuthorId = updatedSeries.AuthorId;
            series.Monitored = updatedSeries.Monitored;

            return series;
        }

        public static List<SeriesResource> ToResource(this IEnumerable<SeriesModel> seriesList)
        {
            return seriesList.Select(ToResource).ToList();
        }

        public static List<SeriesModel> ToModel(this IEnumerable<SeriesResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
