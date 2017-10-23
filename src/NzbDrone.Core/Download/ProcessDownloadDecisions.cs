﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download.Pending;

namespace NzbDrone.Core.Download
{
    public interface IProcessDownloadDecisions
    {
        ProcessedDecisions ProcessDecisions(List<DownloadDecision> decisions);
    }

    public class ProcessDownloadDecisions : IProcessDownloadDecisions
    {
        private readonly IDownloadService _downloadService;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly Logger _logger;

        public ProcessDownloadDecisions(IDownloadService downloadService,
                                        IPrioritizeDownloadDecision prioritizeDownloadDecision,
                                        IPendingReleaseService pendingReleaseService,
                                        Logger logger)
        {
            _downloadService = downloadService;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _pendingReleaseService = pendingReleaseService;
            _logger = logger;
        }

        public ProcessedDecisions ProcessDecisions(List<DownloadDecision> decisions)
        {
            var qualifiedReports = GetQualifiedReports(decisions);
            var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisionsForMovies(qualifiedReports);
            var grabbed = new List<DownloadDecision>();
            var pending = new List<DownloadDecision>();

            foreach (var report in prioritizedDecisions)
            {

                if (report.IsForMovie)
                {
                    var remoteMovie = report.RemoteMovie;

					if (remoteMovie == null || remoteMovie.Movie == null)
					{
						continue;
					}

					List<int> movieIds = new List<int> { remoteMovie.Movie.Id };


					//Skip if already grabbed
					if (grabbed.Select(r => r.RemoteMovie.Movie)
									.Select(e => e.Id)
									.ToList()
									.Intersect(movieIds)
									.Any())
					{
						continue;
					}

                    if (report.TemporarilyRejected)
                    {
                        _pendingReleaseService.Add(report);
                        pending.Add(report);
                        continue;
                    }

                    if (report.Rejections.Any())
                    {
                        _logger.Debug("Rejecting release {0} because {1}", report.ToString(), report.Rejections.First().Reason);
                        continue;
                    }

                   

                    if (pending.Select(r => r.RemoteMovie.Movie)
                            .Select(e => e.Id)
                            .ToList()
                            .Intersect(movieIds)
                            .Any())
                    {
                        continue;
                    }

                    try
                    {
                        _downloadService.DownloadReport(remoteMovie, false);
                        grabbed.Add(report);
                    }
                    catch (Exception e)
                    {
                        //TODO: support for store & forward
                        //We'll need to differentiate between a download client error and an indexer error
                        _logger.Warn(e, "Couldn't add report to download queue. " + remoteMovie);
                    }
                }
                else
                {
                    var remoteEpisode = report.RemoteEpisode;

                    if (remoteEpisode == null || remoteEpisode.Episodes == null)
                    {
                        continue;
                    }

                    var episodeIds = remoteEpisode.Episodes.Select(e => e.Id).ToList();

                    //Skip if already grabbed
                    if (grabbed.SelectMany(r => r.RemoteEpisode.Episodes)
                                    .Select(e => e.Id)
                                    .ToList()
                                    .Intersect(episodeIds)
                                    .Any())
                    {
                        continue;
                    }

                    if (report.TemporarilyRejected)
                    {
                        _pendingReleaseService.Add(report);
                        pending.Add(report);
                        continue;
                    }

                    if (pending.SelectMany(r => r.RemoteEpisode.Episodes)
                            .Select(e => e.Id)
                            .ToList()
                            .Intersect(episodeIds)
                            .Any())
                    {
                        continue;
                    }

                    try
                    {
                        _downloadService.DownloadReport(remoteEpisode);
                        grabbed.Add(report);
                    }
                    catch (Exception e)
                    {
                        //TODO: support for store & forward
                        //We'll need to differentiate between a download client error and an indexer error
                        _logger.Warn(e, "Couldn't add report to download queue. " + remoteEpisode);
                    }
                }
            }

            return new ProcessedDecisions(grabbed, pending, decisions.Where(d => d.Rejected).ToList());
        }

        internal List<DownloadDecision> GetQualifiedReports(IEnumerable<DownloadDecision> decisions)
        {
            //Process both approved and temporarily rejected
            return decisions.Where(c => (c.Approved || c.TemporarilyRejected) && (c.RemoteMovie.Movie != null)).ToList();
        }
    }
}
