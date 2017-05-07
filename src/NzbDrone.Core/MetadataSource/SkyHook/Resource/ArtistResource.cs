using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{

    public class AristResultResource
    {
        public AristResultResource()
        {

        }

        public List<ArtistInfoResource> Items { get; set; }
    }

    public class AlbumResultResource
    {
        public AlbumResultResource()
        {

        }

        public List<AlbumInfoResource> Items { get; set; }
    }

    public class TrackResultResource
    {
        public TrackResultResource()
        {

        }

        public List<TrackInfoResource> Items { get; set; }
    }
    public class ArtistResource
    {
        public ArtistResource()
        {

        }

        public AristResultResource Artists { get; set; }
        public AristResultResource Albums { get; set; }
    }
}
