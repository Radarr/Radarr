import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import translate from 'Utilities/String/translate';
import styles from './MovieFileStatus.css';

function MovieFileStatus(props) {
  const {
    isAvailable,
    isAvailableDate,
    monitored,
    movieFile,
    queueStatus,
    queueState,
    shortDateFormat,
    showRelativeDates,
    timeFormat,
    colorImpairedMode
  } = props;

  const hasMovieFile = !!movieFile;
  const hasReleased = isAvailable;
  const DateConsideredAvailable = getRelativeDate(
    isAvailableDate,
    shortDateFormat,
    showRelativeDates,
    {
      timeFormat,
      timeForToday: false
    }
  );

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
    const quality = movieFile.quality;

    return (
      <div className={styles.center}>
        <span className={styles.ended} />
        {quality.quality.name}
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
      <Label
        title={DateConsideredAvailable}
        kind={kinds.INFO}
      >
        <span className={styles.continuing} />
        {translate('NotAvailable')}
      </Label>
    </div>
  );
}

MovieFileStatus.propTypes = {
  isAvailable: PropTypes.bool,
  isAvailableDate: PropTypes.string,
  monitored: PropTypes.bool.isRequired,
  movieFile: PropTypes.object,
  queueStatus: PropTypes.string,
  queueState: PropTypes.string,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default MovieFileStatus;
