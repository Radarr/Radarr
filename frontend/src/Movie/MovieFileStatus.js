import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import translate from 'Utilities/String/translate';
import styles from './MovieFileStatus.css';

function MovieFileStatus(props) {
  const {
    isAvailable,
    monitored,
    queueStatus,
    queueState,
    statistics,
    colorImpairedMode
  } = props;

  const {
    movieFileCount
  } = statistics;

  const hasMovieFile = movieFileCount > 0;
  const hasReleased = isAvailable;

  if (queueStatus) {
    const queueStatusText = getQueueStatusText(queueStatus, queueState);

    return (
      <div className={styles.center}>
        <span className={styles.queue} />
        {queueStatusText}
      </div>
    );
  }

  if (hasMovieFile) {
    return (
      <div className={styles.center}>
        <span className={styles.ended} />
        Downloaded
      </div>
    );
  }

  if (!monitored) {
    return (
      <div className={classNames(
        styles.center,
        styles.missingUnmonitoredBackground,
        colorImpairedMode && 'colorImpaired'
      )}
      >
        <span className={styles.missingUnmonitored} />
        {translate('NotMonitored')}
      </div>
    );
  }

  if (hasReleased) {
    return (
      <div className={classNames(
        styles.center,
        styles.missingMonitoredBackground,
        colorImpairedMode && 'colorImpaired'
      )}
      >
        <span className={styles.missingMonitored} />
        {translate('Missing')}
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <span className={styles.continuing} />
      {translate('NotAvailable')}
    </div>
  );
}

MovieFileStatus.propTypes = {
  isAvailable: PropTypes.bool,
  monitored: PropTypes.bool.isRequired,
  statistics: PropTypes.object,
  queueStatus: PropTypes.string,
  queueState: PropTypes.string,
  colorImpairedMode: PropTypes.bool.isRequired
};

MovieFileStatus.defaultProps = {
  statistics: {
    movieFileCount: 0
  }
};

export default MovieFileStatus;
