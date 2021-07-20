import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import MovieQuality from 'Movie/MovieQuality';
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
    timeFormat
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
        <Label
          title={queueStatusText}
          kind={kinds.QUEUE}
        >
          {queueStatusText}
        </Label>
      </div>
    );
  }

  if (hasMovieFile) {
    const quality = movieFile.quality;

    return (
      <div className={styles.center}>
        <MovieQuality
          title={quality.quality.name}
          size={movieFile.size}
          quality={quality}
          isMonitored={monitored}
          isCutoffNotMet={movieFile.qualityCutoffNotMet}
        />
      </div>
    );
  }

  if (!monitored) {
    return (
      <div className={styles.center}>
        <Label
          title={translate('NotMonitored')}
          kind={kinds.WARNING}
        >
          {translate('NotMonitored')}
        </Label>
      </div>
    );
  }

  if (hasReleased) {
    return (
      <div className={styles.center}>
        <Label
          title={translate('MovieAvailableButMissing')}
          kind={kinds.DANGER}
        >
          {translate('Missing')}
        </Label>
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Label
        title={DateConsideredAvailable}
        kind={kinds.INFO}
      >
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
  timeFormat: PropTypes.string.isRequired
};

export default MovieFileStatus;
