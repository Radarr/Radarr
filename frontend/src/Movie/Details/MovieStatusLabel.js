import PropTypes from 'prop-types';
import React from 'react';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import translate from 'Utilities/String/translate';
import styles from './MovieStatusLabel.css';

function getMovieStatus(hasFile, isMonitored, isAvailable, queueDetails = false) {

  if (queueDetails.items[0]) {
    const queueStatus = queueDetails.items[0].status;
    const queueState = queueDetails.items[0].trackedDownloadStatus;
    const queueStatusText = getQueueStatusText(queueStatus, queueState);
    return queueStatusText.longText;
  }

  if (hasFile) {
    return translate('Downloaded');
  }

  if (!isMonitored) {
    return translate('Unmonitored');
  }

  if (isAvailable && !hasFile) {
    return translate('Missing');
  }

  return translate('Unreleased');
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
