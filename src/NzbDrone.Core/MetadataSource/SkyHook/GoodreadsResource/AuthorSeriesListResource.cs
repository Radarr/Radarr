using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models the best book in a work, as defined by the Goodreads API.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class AuthorSeriesListResource : GoodreadsResource
    {
        public override string ElementName => "series_works";

        public List<SeriesResource> List { get; private set; }

        public override void Parse(XElement element)
        {
            var pairs = element.Descendants("series_work");
            if (pairs.Any())
            {
                var dict = new Dictionary<long, SeriesResource>();

                foreach (var pair in pairs)
                {
                    var series = new SeriesResource();
                    series.Parse(pair.Element("series"));

                    if (!dict.TryGetValue(series.Id, out var cached))
                    {
                        dict[series.Id] = series;
                        cached = series;
                    }

                    var work = new WorkResource();
                    work.Parse(pair.Element("work"));
                    work.SetSeriesInfo(pair);

                    cached.Works.Add(work);
                }

                List = dict.Values.ToList();
            }
            else
            {
                List = new List<SeriesResource>();
            }
        }
    }
}
