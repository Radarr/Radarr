import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import styles from './MovieStatusLabel.css';

function getMovieStatus(hasFile, isMonitored, isAvailable, queueItem = false) {

  if (queueItem) {
    const queueStatus = queueItem.status;
    const queueState = queueItem.trackedDownloadStatus;
    const queueStatusText = getQueueStatusText(queueStatus, queueState);

    if (queueStatusText) {
      return queueStatusText;
    }
  }

  if (hasFile && !isMonitored) {
    return 'availNotMonitored';
  }

  if (hasFile) {
    return 'ended';
  }

  if (isAvailable && !isMonitored && !hasFile) {
    return 'missingUnmonitored';
  }

  if (isAvailable && !hasFile) {
    return 'missingMonitored';
  }

  return 'continuing';
}

function MovieStatusLabel(props) {
  const {
    hasMovieFiles,
    monitored,
    isAvailable,
    queueItem,
    useLabel,
    colorImpairedMode
  } = props;

  let status = getMovieStatus(hasMovieFiles, monitored, isAvailable, queueItem);
  let statusClass = status;

  if (status === 'availNotMonitored' || status === 'ended') {
    status = 'downloaded';
  }
  if (status === 'missingMonitored' || status === 'missingUnmonitored') {
    status = 'missing';
  }
  if (status === 'continuing') {
    status = 'notAvailable';
  }

  if (queueItem) {
    statusClass = 'queue';
  }

  if (useLabel) {
    let kind = kinds.SUCCESS;

    switch (statusClass) {
      case 'queue':
        kind = kinds.QUEUE;
        break;
      case 'missingMonitored':
        kind = kinds.DANGER;
        break;
      case 'continuing':
        kind = kinds.INFO;
        break;
      case 'availNotMonitored':
        kind = kinds.DEFAULT;
        break;
      case 'missingUnmonitored':
        kind = kinds.WARNING;
        break;
      default:
    }

    return (
      <Label
        kind={kind}
        size={sizes.LARGE}
        colorImpairedMode={colorImpairedMode}
      >
        {translate(firstCharToUpper(status))}
      </Label>
    );
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
  queueItem: PropTypes.object,
  useLabel: PropTypes.bool,
  colorImpairedMode: PropTypes.bool
};

MovieStatusLabel.defaultProps = {
  title: ''
};

export default MovieStatusLabel;
