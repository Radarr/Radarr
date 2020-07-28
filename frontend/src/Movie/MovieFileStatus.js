import PropTypes from 'prop-types';
import React from 'react';
import QueueDetails from 'Activity/Queue/QueueDetails';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import ProgressBar from 'Components/ProgressBar';
import { icons, kinds, sizes } from 'Helpers/Props';
import MovieQuality from 'Movie/MovieQuality';
import styles from './MovieFileStatus.css';

function MovieFileStatus(props) {
  const {
    isAvailable,
    monitored,
    grabbed,
    queueItem,
    movieFile
  } = props;

  const hasMovieFile = !!movieFile;
  const isQueued = !!queueItem;
  const hasReleased = isAvailable;

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
          title="Not Monitored"
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
        title="Not Available"
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
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  movieFile: PropTypes.object
};

export default MovieFileStatus;
