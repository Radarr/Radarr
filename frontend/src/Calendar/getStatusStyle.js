import moment from 'moment';

function getStatusStyle(episodeNumber, hasFile, downloading, startTime, isMonitored) {
  const currentTime = moment();

  if (hasFile) {
    return 'downloaded';
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

  return 'unaired';
}

export default getStatusStyle;
