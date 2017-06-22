using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class RescanArtistCommand : Command
    {

        public string ArtistId { get; set; }

        public override bool SendUpdatesToClient => true;

        public RescanArtistCommand()
        {
            ArtistId = "";
        }

        public RescanArtistCommand(string artistId)
        {
            ArtistId = artistId;
        }
    }
}
