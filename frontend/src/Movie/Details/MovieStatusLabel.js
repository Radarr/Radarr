import PropTypes from 'prop-types';
import React from 'react';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import styles from './MovieStatusLabel.css';

function getMovieStatus(hasFile, isMonitored, isAvailable, queueDetails = false) {

  if (queueDetails.items[0]) {
    const queueStatus = queueDetails.items[0].status;
    const queueState = queueDetails.items[0].trackedDownloadStatus;
    const queueStatusText = getQueueStatusText(queueStatus, queueState);

    if (queueStatusText) {
      return queueStatusText;
    }
  }

  if (hasFile) {
    return 'downloaded';
  }

  if (!isMonitored) {
    return 'unmonitored';
  }

  if (isAvailable && !hasFile) {
    return 'missing';
  }

  return 'notAvailable';
}

function MovieStatusLabel(props) {
  const {
    hasMovieFiles,
    monitored,
    isAvailable,
    queueDetails
  } = props;

  const status = getMovieStatus(hasMovieFiles, monitored, isAvailable, queueDetails);
  let statusClass = status;

  if (queueDetails.items.length) {
    statusClass = 'queue';
  }

  return (
    <span
      className={styles[statusClass]}
    >
      {translate(firstCharToUpper(status))}
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
