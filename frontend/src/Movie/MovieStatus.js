import PropTypes from 'prop-types';
import React from 'react';
import QueueDetails from 'Activity/Queue/QueueDetails';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import { icons, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import MovieQuality from './MovieQuality';
import styles from './MovieStatus.css';

function MovieStatus(props) {
  const {
    isAvailable,
    monitored,
    grabbed,
    queueItem,
    movieFile
  } = props;

  const hasMovieFile = !!movieFile;
  const isQueued = !!queueItem;

  if (isQueued) {
    const {
      sizeleft,
      size
    } = queueItem;

    const progress = size ? (100 - sizeleft / size * 100) : 0;

    return (
      <div className={styles.center}>
        <QueueDetails
          {...queueItem}
          progressBar={
            <ProgressBar
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
          title={translate('MovieIsDownloading')}
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
          title={translate('MovieDownloaded')}
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
          title={translate('MovieIsNotMonitored')}
        />
      </div>
    );
  }

  if (isAvailable) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.MISSING}
          title={translate('MovieMissingFromDisk')}
        />
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Icon
        name={icons.NOT_AIRED}
        title={translate('MovieIsNotAvailable')}
      />
    </div>
  );
}

MovieStatus.propTypes = {
  isAvailable: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  movieFile: PropTypes.object
};

export default MovieStatus;
