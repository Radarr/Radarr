import PropTypes from 'prop-types';
import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import getProgressBarKind from 'Utilities/Movie/getProgressBarKind';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexProgressBar.css';

function MovieIndexProgressBar(props) {
  const {
    monitored,
    status,
    hasFile,
    posterWidth,
    detailedProgressBar,
    queueStatus,
    queueState
  } = props;

  const progress = 100;
  const queueStatusText = getQueueStatusText(queueStatus, queueState);
  let movieStatus = (status === 'released' && hasFile) ? 'downloaded' : status;

  if (movieStatus === 'deleted') {
    movieStatus = 'announced';

    if (hasFile) {
      movieStatus = 'downloaded';
    } else {
      movieStatus = 'released';
    }
  }

  if (movieStatus === 'announced') {
    movieStatus = translate('NotAvailable');
  }

  if (movieStatus === 'released') {
    movieStatus = translate('Missing');
  }

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={styles.progress}
      progress={progress}
      kind={getProgressBarKind(status, monitored, hasFile, queueStatusText)}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={posterWidth}
      text={(queueStatusText) ? queueStatusText.shortText : titleCase(movieStatus)}
    />
  );
}

MovieIndexProgressBar.propTypes = {
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  queueStatus: PropTypes.string,
  queueState: PropTypes.string
};

export default MovieIndexProgressBar;
