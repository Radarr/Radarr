import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import QueueDetails from 'Activity/Queue/QueueDetails';
import formatBytes from 'Utilities/Number/formatBytes';
import Label from 'Components/Label';
import styles from './MovieStatus.css';

function getTooltip(title, quality, size) {
  const revision = quality.revision;

  if (revision.real && revision.real > 0) {
    title += ' [REAL]';
  }

  if (revision.version && revision.version > 1) {
    title += ' [PROPER]';
  }

  if (size) {
    title += ` - ${formatBytes(size)}`;
  }

  return title;
}

function MovieStatus(props) {
  const {
    inCinemas,
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

  if (hasMovieFile && monitored) {
    const quality = movieFile.quality;
    // TODO: Fix on Backend
    // const isCutoffNotMet = movieFile.qualityCutoffNotMet;

    return (
      <div className={styles.center}>
        <Label
          kind={kinds.SUCCESS}
          title={getTooltip('Movie Downloaded', quality, movieFile.size)}
        >
          {quality.quality.name}
        </Label>
      </div>
    );
  } else if (hasMovieFile && !monitored) {
    const quality = movieFile.quality;

    return (
      <div className={styles.center}>
        <Label
          kind={kinds.DISABLED}
          title={getTooltip('Movie Downloaded', quality, movieFile.size)}
        >
          {quality.quality.name}
        </Label>
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
  isAvailable: PropTypes.bool,
  monitored: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  movieFile: PropTypes.object
};

export default MovieStatus;
