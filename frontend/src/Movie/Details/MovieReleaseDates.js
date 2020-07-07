import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import styles from './MovieReleaseDates.css';

function MovieReleaseDates(props) {
  const {
    showRelativeDates,
    shortDateFormat,
    timeFormat,
    inCinemas,
    physicalRelease,
    digitalRelease
  } = props;

  return (
    <div>
      {
        !!inCinemas &&
          <div >
            <span className={styles.dateIcon}>
              <Icon
                name={icons.IN_CINEMAS}
              />
            </span>
            {getRelativeDate(inCinemas, shortDateFormat, showRelativeDates, { timeFormat, timeForToday: false })}
          </div>
      }
      {
        !!physicalRelease &&
          <div >
            <span className={styles.dateIcon}>
              <Icon
                name={icons.DISC}
              />
            </span>
            {getRelativeDate(physicalRelease, shortDateFormat, showRelativeDates, { timeFormat, timeForToday: false })}
          </div>
      }

      {
        !!digitalRelease &&
          <div >
            <span className={styles.dateIcon}>
              <Icon
                name={icons.MOVIE_FILE}
              />
            </span>
            {getRelativeDate(digitalRelease, shortDateFormat, showRelativeDates, { timeFormat, timeForToday: false })}
          </div>
      }
    </div>
  );
}

MovieReleaseDates.propTypes = {
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  inCinemas: PropTypes.string,
  physicalRelease: PropTypes.string,
  digitalRelease: PropTypes.string
};

export default MovieReleaseDates;
