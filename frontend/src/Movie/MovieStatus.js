import PropTypes from 'prop-types';
import React from 'react';
import isBefore from 'Utilities/Date/isBefore';
import { icons, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import QueueDetails from 'Activity/Queue/QueueDetails';
import MovieQuality from 'Movie/MovieQuality';
import Label from 'Components/Label';
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
  const hasReleased = isBefore(inCinemas);

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
          title={quality.quality.name}
          size={movieFile.size}
          quality={quality}
          isCutoffNotMet={isCutoffNotMet}
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
        <Label
          title="Announced"
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
          title="Movie Available, but Missing"
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
        title="Announced"
        kind={kinds.INFO}
      >
        Not Available
      </Label>
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
