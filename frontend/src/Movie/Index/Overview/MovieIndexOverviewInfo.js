import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import MovieIndexOverviewInfoRow from './MovieIndexOverviewInfoRow';
import styles from './MovieIndexOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.movieIndexOverviewInfoRowHeight);

const rows = [
  {
    name: 'monitored',
    showProp: 'showMonitored',
    valueProp: 'monitored'

  },
  {
    name: 'studio',
    showProp: 'showStudio',
    valueProp: 'studio'
  },
  {
    name: 'qualityProfileId',
    showProp: 'showQualityProfile',
    valueProp: 'qualityProfileId'
  },
  {
    name: 'added',
    showProp: 'showAdded',
    valueProp: 'added'
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

  if (name === 'studio') {
    return {
      title: 'Studio',
      iconName: icons.STUDIO,
      label: props.studio
    };
  }

  // if (name === 'qualityProfileId') {
  //   return {
  //     title: 'Quality Profile',
  //     iconName: icons.PROFILE,
  //     label: props.qualityProfile.name
  //   };
  // }

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

function MovieIndexOverviewInfo(props) {
  const {
    height
    // showRelativeDates,
    // shortDateFormat,
    // longDateFormat,
    // timeFormat
  } = props;

  let shownRows = 1;
  const maxRows = Math.floor(height / (infoRowHeight + 4));

  return (
    <div className={styles.infos}>
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
            <MovieIndexOverviewInfoRow
              key={row.name}
              {...infoRowProps}
            />
          );
        })
      }
    </div>
  );
}

MovieIndexOverviewInfo.propTypes = {
  height: PropTypes.number.isRequired,
  showStudio: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  showAdded: PropTypes.bool.isRequired,
  showPath: PropTypes.bool.isRequired,
  showSizeOnDisk: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  studio: PropTypes.string,
  qualityProfile: PropTypes.object.isRequired,
  added: PropTypes.string,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default MovieIndexOverviewInfo;
