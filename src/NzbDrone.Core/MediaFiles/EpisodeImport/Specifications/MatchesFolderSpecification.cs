using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesFolderSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalTrack localTrack)
        {
            if (localTrack.ExistingFile)
            {
                return Decision.Accept();
            }

            var dirInfo = new FileInfo(localTrack.Path).Directory;

            if (dirInfo == null)
            {
                return Decision.Accept();
            }

            throw new System.NotImplementedException("Needs to be implemented");

            //var folderInfo = Parser.Parser.ParseTitle(dirInfo.Name);

            //if (folderInfo == null)
            //{
            //    return Decision.Accept();
            //}
           
            //if (!folderInfo.TrackNumbers.Any())
            //{
            //    return Decision.Accept();
            //}


            //var unexpected = localTrack.ParsedTrackInfo.TrackNumbers.Where(f => !folderInfo.TrackNumbers.Contains(f)).ToList();
            //// TODO: Implement MatchesFolderSpecification
            //if (unexpected.Any())
            //{
            //    _logger.Debug("Unexpected track number(s) in file: {0}", string.Join(", ", unexpected));

            //    if (unexpected.Count == 1)
            //    {
            //        return Decision.Reject("Track Number {0} was unexpected considering the {1} folder name", unexpected.First(), dirInfo.Name);
            //    }

            //    return Decision.Reject("Episode Numbers {0} were unexpected considering the {1} folder name", string.Join(", ", unexpected), dirInfo.Name);
            //}

            return Decision.Accept();
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.ExistingFile)
            {
                return Decision.Accept();
            }

            var dirInfo = new FileInfo(localEpisode.Path).Directory;

            if (dirInfo == null)
            {
                return Decision.Accept();
            }

            var folderInfo = Parser.Parser.ParseTitle(dirInfo.Name);

            if (folderInfo == null)
            {
                return Decision.Accept();
            }

            if (!folderInfo.EpisodeNumbers.Any())
            {
                return Decision.Accept();
            }

            if (folderInfo.FullSeason)
            {
                return Decision.Accept();
            }

            var unexpected = localEpisode.ParsedEpisodeInfo.EpisodeNumbers.Where(f => !folderInfo.EpisodeNumbers.Contains(f)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected episode number(s) in file: {0}", string.Join(", ", unexpected));

                if (unexpected.Count == 1)
                {
                    return Decision.Reject("Episode Number {0} was unexpected considering the {1} folder name", unexpected.First(), dirInfo.Name);
                }

                return Decision.Reject("Episode Numbers {0} were unexpected considering the {1} folder name", string.Join(", ", unexpected), dirInfo.Name);
            }

            return Decision.Accept();
        }
    }
}
