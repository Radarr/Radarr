import _ from 'lodash';

function monitorSeasons(seasons, startingSeason) {
  seasons.forEach((season) => {
    if (season.seasonNumber >= startingSeason) {
      season.monitored = true;
    } else {
      season.monitored = false;
    }
  });
}

function getMonitoringOptions(albums, monitor) {
  if (!albums.length) {
    return {
      albums: [],
      options: {
        ignoreEpisodesWithFiles: false,
        ignoreEpisodesWithoutFiles: false
      }
    };
  }

  const firstSeason = _.minBy(_.reject(albums, { seasonNumber: 0 }), 'seasonNumber').seasonNumber;
  const lastSeason = _.maxBy(albums, 'seasonNumber').seasonNumber;

  monitorSeasons(albums, firstSeason);

  const monitoringOptions = {
    ignoreEpisodesWithFiles: false,
    ignoreEpisodesWithoutFiles: false
  };

  switch (monitor) {
    case 'future':
      monitoringOptions.ignoreEpisodesWithFiles = true;
      monitoringOptions.ignoreEpisodesWithoutFiles = true;
      break;
    case 'latest':
      monitorSeasons(albums, lastSeason);
      break;
    case 'first':
      monitorSeasons(albums, lastSeason + 1);
      _.find(albums, { seasonNumber: firstSeason }).monitored = true;
      break;
    case 'missing':
      monitoringOptions.ignoreEpisodesWithFiles = true;
      break;
    case 'existing':
      monitoringOptions.ignoreEpisodesWithoutFiles = true;
      break;
    case 'none':
      monitorSeasons(albums, lastSeason + 1);
      break;
    default:
      break;
  }

  return {
    seasons: _.map(albums, (season) => {
      return _.pick(season, [
        'seasonNumber',
        'monitored'
      ]);
    }),
    options: monitoringOptions
  };
}

export default getMonitoringOptions;
