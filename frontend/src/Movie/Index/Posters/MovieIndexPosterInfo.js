import PropTypes from 'prop-types';
import React from 'react';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import styles from './MovieIndexPosterInfo.css';

function MovieIndexPosterInfo(props) {
  const {
    studio,
    qualityProfile,
    showQualityProfile,
    added,
    path,
    sizeOnDisk,
    sortKey,
    showRelativeDates,
    shortDateFormat,
    timeFormat
  } = props;

  if (sortKey === 'studio' && studio) {
    return (
      <div className={styles.info}>
        {studio}
      </div>
    );
  }

  if (sortKey === 'qualityProfileId' && !showQualityProfile) {
    return (
      <div className={styles.info}>
        {qualityProfile.name}
      </div>
    );
  }

  if (sortKey === 'added' && added) {
    const addedDate = getRelativeDate(
      added,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false
      }
    );

    return (
      <div className={styles.info}>
        {`Added ${addedDate}`}
      </div>
    );
  }

  if (sortKey === 'path') {
    return (
      <div className={styles.info}>
        {path}
      </div>
    );
  }

  if (sortKey === 'sizeOnDisk') {
    return (
      <div className={styles.info}>
        {formatBytes(sizeOnDisk)}
      </div>
    );
  }

  return null;
}

MovieIndexPosterInfo.propTypes = {
  studio: PropTypes.string,
  showQualityProfile: PropTypes.bool.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  added: PropTypes.string,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default MovieIndexPosterInfo;
