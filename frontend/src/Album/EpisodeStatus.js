import PropTypes from 'prop-types';
import React from 'react';
import isBefore from 'Utilities/Date/isBefore';
import { icons, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import QueueDetails from 'Activity/Queue/QueueDetails';
import TrackQuality from './TrackQuality';
import styles from './EpisodeStatus.css';

function EpisodeStatus(props) {
  const {
    airDateUtc,
    monitored,
    grabbed,
    queueItem,
    trackFile
  } = props;

  const hasTrackFile = !!trackFile;
  const isQueued = !!queueItem;
  const hasAired = isBefore(airDateUtc);

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
              title={`Album is downloading - ${progress.toFixed(1)}% ${queueItem.title}`}
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
          title="Album is downloading"
        />
      </div>
    );
  }

  if (hasTrackFile) {
    const quality = trackFile.quality;
    const isCutoffNotMet = trackFile.qualityCutoffNotMet;

    return (
      <div className={styles.center}>
        <TrackQuality
          quality={quality}
          size={trackFile.size}
          isCutoffNotMet={isCutoffNotMet}
          title="Track Downloaded"
        />
      </div>
    );
  }

  if (!airDateUtc) {
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
          title="Album is not monitored"
        />
      </div>
    );
  }

  if (hasAired) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.MISSING}
          title="Track missing from disk"
        />
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Icon
        name={icons.NOT_AIRED}
        title="Album has not aired"
      />
    </div>
  );
}

EpisodeStatus.propTypes = {
  airDateUtc: PropTypes.string,
  monitored: PropTypes.bool,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  trackFile: PropTypes.object
};

export default EpisodeStatus;
