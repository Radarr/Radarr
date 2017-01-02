using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecision
    {
        public RemoteEpisode RemoteEpisode { get; private set; }

        public RemoteMovie RemoteMovie { get; private set; }

        public bool IsForMovie = false;
        public IEnumerable<Rejection> Rejections { get; private set; }

        public bool Approved => !Rejections.Any();

        public bool TemporarilyRejected
        {
            get
            {
                return Rejections.Any() && Rejections.All(r => r.Type == RejectionType.Temporary);
            }
        }

        public bool Rejected
        {
            get
            {
                return Rejections.Any() && Rejections.Any(r => r.Type == RejectionType.Permanent);
            }
        }

        public DownloadDecision(RemoteEpisode episode, params Rejection[] rejections)
        {
            RemoteEpisode = episode;
            RemoteMovie = new RemoteMovie
            {
                Release = episode.Release,
                ParsedEpisodeInfo = episode.ParsedEpisodeInfo
            };
            Rejections = rejections.ToList();
        }

        public DownloadDecision(RemoteMovie movie, params Rejection[] rejections)
        {
            RemoteMovie = movie;
            RemoteEpisode = new RemoteEpisode
            {
                Release = movie.Release,
                ParsedEpisodeInfo = movie.ParsedEpisodeInfo
            };
            IsForMovie = true;
            Rejections = rejections.ToList();
        }
        
        public override string ToString()
        {
            if (Approved)
            {
                return "[OK] " + RemoteEpisode;
            }

            return "[Rejected " + Rejections.Count() + "]" + RemoteEpisode;
        }
    }
}