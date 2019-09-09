import PropTypes from 'prop-types';
import React from 'react';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import ArtistIndexOverviewInfoRow from './ArtistIndexOverviewInfoRow';
import styles from './ArtistIndexOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.artistIndexOverviewInfoRowHeight);

const rows = [
  {
    name: 'monitored',
    showProp: 'showMonitored',
    valueProp: 'monitored'

  },
  {
    name: 'qualityProfileId',
    showProp: 'showQualityProfile',
    valueProp: 'qualityProfileId'
  },
  {
    name: 'lastAlbum',
    showProp: 'showLastAlbum',
    valueProp: 'lastAlbum'
  },
  {
    name: 'added',
    showProp: 'showAdded',
    valueProp: 'added'
  },
  {
    name: 'albumCount',
    showProp: 'showAlbumCount',
    valueProp: 'albumCount'
  },
  {
    name: 'path',
    showProp: 'showPath',
    valueProp: 'path'
  },
  {
    name: 'sizeOnDisk',
    showProp: 'showSizeOnDisk',
    valueProp: 'sizeOnDisk'
  }
];

function isVisible(row, props) {
  const {
    name,
    showProp,
    valueProp
  } = row;

  if (props[valueProp] == null) {
    return false;
  }

  return props[showProp] || props.sortKey === name;
}

function getInfoRowProps(row, props) {
  const { name } = row;

  if (name === 'monitored') {
    const monitoredText = props.monitored ? 'Monitored' : 'Unmonitored';

    return {
      title: monitoredText,
      iconName: props.monitored ? icons.MONITORED : icons.UNMONITORED,
      label: monitoredText
    };
  }

  if (name === 'qualityProfileId') {
    return {
      title: 'Quality Profile',
      iconName: icons.PROFILE,
      label: props.qualityProfile.name
    };
  }

  if (name === 'lastAlbum') {
    const {
      lastAlbum,
      showRelativeDates,
      shortDateFormat,
      timeFormat
    } = props;

    return {
      title: `Last Album: ${lastAlbum.title}`,
      iconName: icons.CALENDAR,
      label: getRelativeDate(
        lastAlbum.releaseDate,
        shortDateFormat,
        showRelativeDates,
        {
          timeFormat,
          timeForToday: true
        }
      )
    };
  }

  if (name === 'added') {
    const {
      added,
      showRelativeDates,
      shortDateFormat,
      longDateFormat,
      timeFormat
    } = props;

    return {
      title: `Added: ${formatDateTime(added, longDateFormat, timeFormat)}`,
      iconName: icons.ADD,
      label: getRelativeDate(
        added,
        shortDateFormat,
        showRelativeDates,
        {
          timeFormat,
          timeForToday: true
        }
      )
    };
  }

  if (name === 'albumCount') {
    const { albumCount } = props;
    let albums = '1 album';

    if (albumCount === 0) {
      albums = 'No albums';
    } else if (albumCount > 1) {
      albums = `${albumCount} albums`;
    }

    return {
      title: 'Album Count',
      iconName: icons.CIRCLE,
      label: albums
    };
  }

  if (name === 'path') {
    return {
      title: 'Path',
      iconName: icons.FOLDER,
      label: props.path
    };
  }

  if (name === 'sizeOnDisk') {
    return {
      title: 'Size on Disk',
      iconName: icons.DRIVE,
      label: formatBytes(props.sizeOnDisk)
    };
  }
}

function ArtistIndexOverviewInfo(props) {
  const {
    height,
    nextAiring,
    showRelativeDates,
    shortDateFormat,
    longDateFormat,
    timeFormat
  } = props;

  let shownRows = 1;

  const maxRows = Math.floor(height / (infoRowHeight + 4));

  return (
    <div className={styles.infos}>
      {
        !!nextAiring &&
        <ArtistIndexOverviewInfoRow
          title={formatDateTime(nextAiring, longDateFormat, timeFormat)}
          iconName={icons.SCHEDULED}
          label={getRelativeDate(
            nextAiring,
            shortDateFormat,
            showRelativeDates,
            {
              timeFormat,
              timeForToday: true
            }
          )}
        />
      }

      {
        rows.map((row) => {
          if (!isVisible(row, props)) {
            return null;
          }

          if (shownRows >= maxRows) {
            return null;
          }

          shownRows++;

          const infoRowProps = getInfoRowProps(row, props);

          return (
            <ArtistIndexOverviewInfoRow
              key={row.name}
              {...infoRowProps}
            />
          );
        })
      }
    </div>
  );
}

ArtistIndexOverviewInfo.propTypes = {
  height: PropTypes.number.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  showAdded: PropTypes.bool.isRequired,
  showAlbumCount: PropTypes.bool.isRequired,
  showPath: PropTypes.bool.isRequired,
  showSizeOnDisk: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  nextAiring: PropTypes.string,
  qualityProfile: PropTypes.object.isRequired,
  lastAlbum: PropTypes.object,
  added: PropTypes.string,
  albumCount: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default ArtistIndexOverviewInfo;
