using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Series
{
    [Obsolete("SeriesResource is Obsolete, Remove with new UI")]
    public class SeriesResource : RestResource
    {
        public SeriesResource()
        {
            Title = "Series Endpoint Obsolete";
        }
        
        //View Only
        public string Title { get; set; }
    }
    
}
