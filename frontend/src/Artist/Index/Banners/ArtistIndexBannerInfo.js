import PropTypes from 'prop-types';
import React from 'react';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import styles from './ArtistIndexBannerInfo.css';

function ArtistIndexBannerInfo(props) {
  const {
    qualityProfile,
    showQualityProfile,
    previousAiring,
    added,
    albumCount,
    path,
    sizeOnDisk,
    sortKey,
    showRelativeDates,
    shortDateFormat,
    timeFormat
  } = props;

  if (sortKey === 'qualityProfileId' && !showQualityProfile) {
    return (
      <div className={styles.info}>
        {qualityProfile.name}
      </div>
    );
  }

  if (sortKey === 'previousAiring' && previousAiring) {
    return (
      <div className={styles.info}>
        {
          getRelativeDate(
            previousAiring,
            shortDateFormat,
            showRelativeDates,
            {
              timeFormat,
              timeForToday: true
            }
          )
        }
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

  if (sortKey === 'albumCount') {
    let albums = '1 album';

    if (albumCount === 0) {
      albums = 'No albums';
    } else if (albumCount > 1) {
      albums = `${albumCount} albums`;
    }

    return (
      <div className={styles.info}>
        {albums}
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

ArtistIndexBannerInfo.propTypes = {
  qualityProfile: PropTypes.object.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  previousAiring: PropTypes.string,
  added: PropTypes.string,
  albumCount: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default ArtistIndexBannerInfo;
