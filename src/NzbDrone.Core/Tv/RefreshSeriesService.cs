using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DataAugmentation.DailySeries;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class RefreshSeriesService : IExecute<RefreshSeriesCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly IRefreshEpisodeService _refreshEpisodeService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDailySeriesService _dailySeriesService;
        private readonly IDiskScanService _diskScanService;
        private readonly Logger _logger;

        public RefreshSeriesService(ISeriesService seriesService,
                                    IRefreshEpisodeService refreshEpisodeService,
                                    IEventAggregator eventAggregator,
                                    IDailySeriesService dailySeriesService,
                                    IDiskScanService diskScanService,
                                    Logger logger)
        {
            _seriesService = seriesService;
            _refreshEpisodeService = refreshEpisodeService;
            _eventAggregator = eventAggregator;
            _dailySeriesService = dailySeriesService;
            _diskScanService = diskScanService;
            _logger = logger;
        }


        private List<Season> UpdateSeasons(Series series, Series seriesInfo)
        {
            var seasons = seriesInfo.Seasons.DistinctBy(s => s.SeasonNumber).ToList();

            foreach (var season in seasons)
            {
                var existingSeason = series.Seasons.FirstOrDefault(s => s.SeasonNumber == season.SeasonNumber);

                //Todo: Should this should use the previous season's monitored state?
                if (existingSeason == null)
                {
                    if (season.SeasonNumber == 0)
                    {
                        season.Monitored = false;
                        continue;
                    }

                    _logger.Debug("New season ({0}) for series: [{1}] {2}, setting monitored to true", season.SeasonNumber, series.TvdbId, series.Title);
                    season.Monitored = true;
                }

                else
                {
                    season.Monitored = existingSeason.Monitored;
                }
            }

            return seasons;
        }

        public void Execute(RefreshSeriesCommand message)
        {
            _eventAggregator.PublishEvent(new SeriesRefreshStartingEvent(message.Trigger == CommandTrigger.Manual));

            if (message.SeriesId.HasValue)
            {
                var series = _seriesService.GetSeries(message.SeriesId.Value);
            }
            else
            {
                var allSeries = _seriesService.GetAllSeries().OrderBy(c => c.SortTitle).ToList();

                foreach (var series in allSeries)
                {
                    if (message.Trigger == CommandTrigger.Manual)
                    {
                        try
                        {
                            //RefreshSeriesInfo(series);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", series);
                        }
                    }

                    else
                    {
                        try
                        {
                            _logger.Info("Skipping refresh of series: {0}", series.Title);
                            //_diskScanService.Scan(series);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't rescan series {0}", series);
                        }
                    }
                }
            }
        }
    }
}
