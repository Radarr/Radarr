/* eslint max-params: 0 */
import moment from 'moment';

function getStatusStyle(hasFile, downloading, startTime, isMonitored) {
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

  if (startTime.isBefore(currentTime) && !hasFile) {
    return 'missing';
  }

  return 'unaired';
}

export default getStatusStyle;
