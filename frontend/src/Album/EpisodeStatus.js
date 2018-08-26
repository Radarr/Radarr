import PropTypes from 'prop-types';
import React from 'react';
import isBefore from 'Utilities/Date/isBefore';
import { icons, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import QueueDetails from 'Activity/Queue/QueueDetails';
import EpisodeQuality from './EpisodeQuality';
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
      <div className={styles.center} title="Album is downloading">
        <Icon
          name={icons.DOWNLOADING}
        />
      </div>
    );
  }

  if (hasTrackFile) {
    const quality = trackFile.quality;
    const isCutoffNotMet = trackFile.qualityCutoffNotMet;

    return (
      <div className={styles.center}>
        <EpisodeQuality
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
      <div className={styles.center} title="TBA">
        <Icon
          name={icons.TBA}
        />
      </div>
    );
  }

  if (!monitored) {
    return (
      <div className={styles.center} title="Album is not monitored">
        <Icon
          name={icons.UNMONITORED}
        />
      </div>
    );
  }

  if (hasAired) {
    return (
      <div className={styles.center} title="Track missing from disk">
        <Icon
          name={icons.MISSING}
        />
      </div>
    );
  }

  return (
    <div className={styles.center} title="Album has not aired">
      <Icon
        name={icons.NOT_AIRED}
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
