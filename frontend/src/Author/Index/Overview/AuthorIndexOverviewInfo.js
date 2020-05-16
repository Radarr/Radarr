import PropTypes from 'prop-types';
import React from 'react';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import AuthorIndexOverviewInfoRow from './AuthorIndexOverviewInfoRow';
import styles from './AuthorIndexOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.authorIndexOverviewInfoRowHeight);

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
    name: 'lastBook',
    showProp: 'showLastBook',
    valueProp: 'lastBook'
  },
  {
    name: 'added',
    showProp: 'showAdded',
    valueProp: 'added'
  },
  {
    name: 'bookCount',
    showProp: 'showBookCount',
    valueProp: 'bookCount'
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

  if (name === 'lastBook') {
    const {
      lastBook,
      showRelativeDates,
      shortDateFormat,
      timeFormat
    } = props;

    return {
      title: `Last Book: ${lastBook.title}`,
      iconName: icons.CALENDAR,
      label: getRelativeDate(
        lastBook.releaseDate,
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

  if (name === 'bookCount') {
    const { bookCount } = props;
    let books = '1 book';

    if (bookCount === 0) {
      books = 'No books';
    } else if (bookCount > 1) {
      books = `${bookCount} books`;
    }

    return {
      title: 'Book Count',
      iconName: icons.BOOK,
      label: books
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

function AuthorIndexOverviewInfo(props) {
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
        <AuthorIndexOverviewInfoRow
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
            <AuthorIndexOverviewInfoRow
              key={row.name}
              {...infoRowProps}
            />
          );
        })
      }
    </div>
  );
}

AuthorIndexOverviewInfo.propTypes = {
  height: PropTypes.number.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  showAdded: PropTypes.bool.isRequired,
  showBookCount: PropTypes.bool.isRequired,
  showPath: PropTypes.bool.isRequired,
  showSizeOnDisk: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  nextAiring: PropTypes.string,
  qualityProfile: PropTypes.object.isRequired,
  lastBook: PropTypes.object,
  added: PropTypes.string,
  bookCount: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default AuthorIndexOverviewInfo;
