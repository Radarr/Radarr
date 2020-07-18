import PropTypes from 'prop-types';
import React from 'react';
import styles from './MovieStatusLabel.css';

function getMovieStatus(hasFile, isMonitored, isAvailable, queueDetails = false) {

  if (queueDetails.items[0]) {
    const queueStatus = queueDetails.items[0].status;
    const trackedDownloadStatus = queueDetails.items[0].trackedDownloadStatus;

    switch (true) {
      case queueStatus !== 'completed':
        switch (queueStatus) {
          case 'queue':
          case 'paused':
          case 'failed':
            return `Downloading: ${queueStatus[0].toUpperCase()}${queueStatus.substr(1).toLowerCase()}`;
          case 'delay':
            return 'Downloading: Pending';
          case 'DownloadClientUnavailable':
          case 'warning':
            return 'Downloading: Error';
          case 'downloading':
            return queueStatus[0].toUpperCase() + queueStatus.substr(1).toLowerCase();
          default:
        }
        break;
      case queueStatus === 'completed':
        switch (trackedDownloadStatus) {
          case 'importPending':
            return 'Downloaded: Pending';
          case 'importing':
            return 'Downloaded: Importing';
          case 'failedPending':
            return 'Downloaded: Waiting';
          default:
        }
        break;
      default:
    }
  }

  if (hasFile) {
    return 'Downloaded';
  }

  if (!isMonitored) {
    return 'Unmonitored';
  }

  if (isAvailable && !hasFile) {
    return 'Missing';
  }

  return 'Unreleased';
}

function MovieStatusLabel(props) {
  const {
    hasMovieFiles,
    monitored,
    isAvailable,
    queueDetails
  } = props;

  const status = getMovieStatus(hasMovieFiles, monitored, isAvailable, queueDetails);
  const statusClass = status.replace('Downloading: ', '').replace('Downloaded: ', '').toLowerCase();

  return (
    <span
      className={styles[statusClass]}
    >
      {status}
    </span>
  );
}

MovieStatusLabel.propTypes = {
  hasMovieFiles: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  isAvailable: PropTypes.bool.isRequired,
  queueDetails: PropTypes.object
};

MovieStatusLabel.defaultProps = {
  title: ''
};

export default MovieStatusLabel;
