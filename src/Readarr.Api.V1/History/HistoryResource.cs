using System;
using System.Collections.Generic;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using Readarr.Api.V1.Albums;
using Readarr.Api.V1.Artist;
using Readarr.Http.REST;

namespace Readarr.Api.V1.History
{
    public class HistoryResource : RestResource
    {
        public int BookId { get; set; }
        public int AuthorId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public bool QualityCutoffNotMet { get; set; }
        public DateTime Date { get; set; }
        public string DownloadId { get; set; }

        public HistoryEventType EventType { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public AlbumResource Album { get; set; }
        public ArtistResource Artist { get; set; }
    }

    public static class HistoryResourceMapper
    {
        public static HistoryResource ToResource(this NzbDrone.Core.History.History model)
        {
            if (model == null)
            {
                return null;
            }

            return new HistoryResource
            {
                Id = model.Id,

                BookId = model.BookId,
                AuthorId = model.AuthorId,
                SourceTitle = model.SourceTitle,
                Quality = model.Quality,

                //QualityCutoffNotMet
                Date = model.Date,
                DownloadId = model.DownloadId,

                EventType = model.EventType,

                Data = model.Data

                //Episode
                //Series
            };
        }
    }
}
