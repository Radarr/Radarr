/* eslint max-params: 0 */
import PropTypes from 'prop-types';
import React from 'react';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import Icon from 'Components/Icon';
import styles from './ArtistIndexOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.artistIndexOverviewInfoRowHeight);

function isVisible(name, show, value, sortKey, index) {
  if (value == null) {
    return false;
  }

  return show || sortKey === name;
}

function ArtistIndexOverviewInfo(props) {
  const {
    height,
    showMonitored,
    showQualityProfile,
    showAdded,
    showAlbumCount,
    showPath,
    showSizeOnDisk,
    monitored,
    nextAiring,
    qualityProfile,
    added,
    albumCount,
    path,
    sizeOnDisk,
    sortKey,
    showRelativeDates,
    shortDateFormat,
    timeFormat
  } = props;

  let albums = '1 album';

  if (albumCount === 0) {
    albums = 'No albums';
  } else if (albumCount > 1) {
    albums = `${albumCount} albums`;
  }

  const maxRows = Math.floor(height / (infoRowHeight + 4));
  const monitoredText = monitored ? 'Monitored' : 'Unmonitored';

  return (
    <div className={styles.infos}>
      {
        !!nextAiring &&
          <div
            className={styles.info}
            title="Next Airing"
          >
            <Icon
              className={styles.icon}
              name={icons.SCHEDULED}
              size={14}
            />

            {
              getRelativeDate(
                nextAiring,
                shortDateFormat,
                showRelativeDates,
                {
                  timeFormat,
                  timeForToday: true
                }
              )
            }
          </div>
      }

      {
        isVisible('monitored', showMonitored, monitored, sortKey) && maxRows > 1 &&
          <div
            className={styles.info}
            title={monitoredText}
          >
            <Icon
              className={styles.icon}
              name={monitored ? icons.MONITORED : icons.UNMONITORED}
              size={14}
            />

            {monitoredText}
          </div>
      }

      {
        isVisible('qualityProfileId', showQualityProfile, qualityProfile, sortKey) && maxRows > 2 &&
          <div
            className={styles.info}
            title="Quality Profile"
          >
            <Icon
              className={styles.icon}
              name={icons.PROFILE}
              size={14}
            />

            {qualityProfile.name}
          </div>
      }

      {
        isVisible('added', showAdded, added, sortKey) && maxRows > 3 &&
          <div
            className={styles.info}
            title="Date Added"
          >
            <Icon
              className={styles.icon}
              name={icons.ADD}
              size={14}
            />

            {
              getRelativeDate(
                added,
                shortDateFormat,
                showRelativeDates,
                {
                  timeFormat,
                  timeForToday: true
                }
              )
            }
          </div>
      }

      {
        isVisible('albumCount', showAlbumCount, albumCount, sortKey) && maxRows > 4 &&
          <div
            className={styles.info}
            title="Album Count"
          >
            <Icon
              className={styles.icon}
              name={icons.CIRCLE}
              size={14}
            />

            {albums}
          </div>
      }

      {
        isVisible('path', showPath, path, sortKey) && maxRows > 5 &&
          <div
            className={styles.info}
            title="Path"
          >
            <Icon
              className={styles.icon}
              name={icons.FOLDER}
              size={14}
            />

            {path}
          </div>
      }

      {
        isVisible('sizeOnDisk', showSizeOnDisk, sizeOnDisk, sortKey) && maxRows > 6 &&
          <div
            className={styles.info}
            title="Size on Disk"
          >
            <Icon
              className={styles.icon}
              name={icons.DRIVE}
              size={14}
            />

            {formatBytes(sizeOnDisk)}
          </div>
      }

    </div>
  );
}

ArtistIndexOverviewInfo.propTypes = {
  height: PropTypes.number.isRequired,
  showNetwork: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  showAdded: PropTypes.bool.isRequired,
  showAlbumCount: PropTypes.bool.isRequired,
  showPath: PropTypes.bool.isRequired,
  showSizeOnDisk: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  nextAiring: PropTypes.string,
  qualityProfile: PropTypes.object.isRequired,
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

export default ArtistIndexOverviewInfo;
