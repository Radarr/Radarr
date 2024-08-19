import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import styles from './MovieStatusLabel.css';

function getMovieStatus(status, hasFile, isMonitored, isAvailable, queueItem = false) {
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

  if (status === 'deleted') {
    return 'deleted';
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
    status,
    hasMovieFiles,
    monitored,
    isAvailable,
    queueItem,
    useLabel,
    colorImpairedMode
  } = props;

  let movieStatus = getMovieStatus(status, hasMovieFiles, monitored, isAvailable, queueItem);
  let statusClass = movieStatus;

  if (movieStatus === 'availNotMonitored' || movieStatus === 'ended') {
    movieStatus = 'downloaded';
  } else if (movieStatus === 'missingMonitored' || movieStatus === 'missingUnmonitored') {
    movieStatus = 'missing';
  } else if (movieStatus === 'continuing') {
    movieStatus = 'notAvailable';
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
      case 'deleted':
        kind = kinds.INVERSE;
        break;
      default:
    }

    return (
      <Label
        kind={kind}
        size={sizes.LARGE}
        colorImpairedMode={colorImpairedMode}
      >
        {translate(firstCharToUpper(movieStatus))}
      </Label>
    );
  }

  return (
    <span
      className={styles[statusClass]}
    >
      {translate(firstCharToUpper(movieStatus))}
    </span>
  );
}

MovieStatusLabel.propTypes = {
  status: PropTypes.string.isRequired,
  hasMovieFiles: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  isAvailable: PropTypes.bool.isRequired,
  queueItem: PropTypes.object,
  useLabel: PropTypes.bool,
  colorImpairedMode: PropTypes.bool
};

MovieStatusLabel.defaultProps = {
  useLabel: false,
  colorImpairedMode: false
};

export default MovieStatusLabel;
