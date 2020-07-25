import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import MovieQuality from 'Movie/MovieQuality';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import translate from 'Utilities/String/translate';
import styles from './MovieFileStatus.css';

function MovieFileStatus(props) {
  const {
    isAvailable,
    monitored,
    movieFile,
    queueStatus,
    queueState
  } = props;

  const hasMovieFile = !!movieFile;
  const hasReleased = isAvailable;

  if (queueStatus) {
    const queueStatusText = getQueueStatusText(queueStatus, queueState);

    return (
      <div className={styles.center}>
        <Label
          title={queueStatusText.longText}
          kind={kinds.QUEUE}
        >
          {queueStatusText.shortText}
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
          Not Monitored
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
          Missing
        </Label>
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Label
        title={translate('NotAvailable')}
        kind={kinds.INFO}
      >
        Not Available
      </Label>
    </div>
  );
}

MovieFileStatus.propTypes = {
  isAvailable: PropTypes.bool,
  monitored: PropTypes.bool.isRequired,
  movieFile: PropTypes.object,
  queueStatus: PropTypes.string,
  queueState: PropTypes.string
};

export default MovieFileStatus;
