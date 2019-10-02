import PropTypes from 'prop-types';
import React from 'react';
import isBefore from 'Utilities/Date/isBefore';
import { icons, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import QueueDetails from 'Activity/Queue/QueueDetails';
import MovieQuality from './MovieQuality';
import styles from './MovieStatus.css';

function MovieStatus(props) {
  const {
    inCinemas,
    monitored,
    grabbed,
    queueItem,
    movieFile
  } = props;

  const hasMovieFile = !!movieFile;
  const isQueued = !!queueItem;
  const hasAired = isBefore(inCinemas);

  if (isQueued) {
    const {
      sizeleft,
      size
    } = queueItem;

    const progress = (100 - sizeleft / size * 100);

    return (
      <div className={styles.center}>
        <QueueDetails
          {...queueItem}
          progressBar={
            <ProgressBar
              title={`Movie is downloading - ${progress.toFixed(1)}% ${queueItem.title}`}
              progress={progress}
              kind={kinds.PURPLE}
              size={sizes.MEDIUM}
            />
          }
        />
      </div>
    );
  }

  if (grabbed) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.DOWNLOADING}
          title="Movie is downloading"
        />
      </div>
    );
  }

  if (hasMovieFile) {
    const quality = movieFile.quality;
    const isCutoffNotMet = movieFile.qualityCutoffNotMet;

    return (
      <div className={styles.center}>
        <MovieQuality
          quality={quality}
          size={movieFile.size}
          isCutoffNotMet={isCutoffNotMet}
          title="Movie Downloaded"
        />
      </div>
    );
  }

  if (!inCinemas) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.TBA}
          title="TBA"
        />
      </div>
    );
  }

  if (!monitored) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.UNMONITORED}
          kind={kinds.DISABLED}
          title="Movie is not monitored"
        />
      </div>
    );
  }

  if (hasAired) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.MISSING}
          title="Movie missing from disk"
        />
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Icon
        name={icons.NOT_AIRED}
        title="Movie has not aired"
      />
    </div>
  );
}

MovieStatus.propTypes = {
  inCinemas: PropTypes.string,
  monitored: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  movieFile: PropTypes.object
};

export default MovieStatus;
