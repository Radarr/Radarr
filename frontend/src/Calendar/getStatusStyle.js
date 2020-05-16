/* eslint max-params: 0 */
import moment from 'moment';

function getStatusStyle(episodeNumber, downloading, startTime, isMonitored, percentOfBooks) {
  const currentTime = moment();

  if (percentOfBooks === 100) {
    return 'downloaded';
  }

  if (percentOfBooks > 0) {
    return 'partial';
  }

  if (downloading) {
    return 'downloading';
  }

  if (!isMonitored) {
    return 'unmonitored';
  }

  if (currentTime.isAfter(startTime)) {
    return 'missing';
  }

  return 'unreleased';
}

export default getStatusStyle;
