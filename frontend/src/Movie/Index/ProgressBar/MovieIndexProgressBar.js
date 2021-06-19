import PropTypes from 'prop-types';
import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import getStatusStyle from 'Utilities/Movie/getStatusStyle';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexProgressBar.css';

function MovieIndexProgressBar(props) {
  const {
    monitored,
    status,
    hasFile,
    isAvailable,
    posterWidth,
    detailedProgressBar,
    queueStatus,
    queueState
  } = props;

  const progress = 100;
  const queueStatusText = getQueueStatusText(queueStatus, queueState);
  let movieStatus = (status === 'released' && hasFile) ? 'downloaded' : status;

  if (movieStatus === 'deleted') {
    movieStatus = 'Missing';

    if (hasFile) {
      movieStatus = 'Downloaded';
    }
  } else if (hasFile) {
    movieStatus = 'Downloaded';
  } else if (isAvailable && !hasFile) {
    movieStatus = 'Missing';
  } else {
    movieStatus = 'NotAvailable';
  }

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={styles.progress}
      progress={progress}
      kind={getStatusStyle(status, monitored, hasFile, isAvailable, 'kinds', queueStatusText)}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={posterWidth}
      text={(queueStatusText) ? queueStatusText : translate(movieStatus)}
    />
  );
}

MovieIndexProgressBar.propTypes = {
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  isAvailable: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  queueStatus: PropTypes.string,
  queueState: PropTypes.string
};

export default MovieIndexProgressBar;
