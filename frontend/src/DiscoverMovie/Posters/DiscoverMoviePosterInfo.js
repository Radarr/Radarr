import PropTypes from 'prop-types';
import React from 'react';
import HeartRating from 'Components/HeartRating';
import { getMovieStatusDetails } from 'Movie/MovieStatus';
import formatRuntime from 'Utilities/Date/formatRuntime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import styles from './DiscoverMoviePosterInfo.css';

function DiscoverMoviePosterInfo(props) {
  const {
    status,
    studio,
    inCinemas,
    digitalRelease,
    physicalRelease,
    certification,
    runtime,
    ratings,
    sortKey,
    showRelativeDates,
    shortDateFormat,
    timeFormat
  } = props;

  if (sortKey === 'status' && status) {
    return (
      <div className={styles.info}>
        {getMovieStatusDetails(status).title}
      </div>
    );
  }

  if (sortKey === 'studio' && studio) {
    return (
      <div className={styles.info}>
        {studio}
      </div>
    );
  }

  if (sortKey === 'inCinemas' && inCinemas) {
    const inCinemasDate = getRelativeDate(
      inCinemas,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false
      }
    );

    return (
      <div className={styles.info}>
        {`In Cinemas ${inCinemasDate}`}
      </div>
    );
  }

  if (sortKey === 'digitalRelease' && digitalRelease) {
    const digitalReleaseDate = getRelativeDate(
      digitalRelease,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false
      }
    );

    return (
      <div className={styles.info}>
        {`Digital ${digitalReleaseDate}`}
      </div>
    );
  }

  if (sortKey === 'physicalRelease' && physicalRelease) {
    const physicalReleaseDate = getRelativeDate(
      physicalRelease,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false
      }
    );

    return (
      <div className={styles.info}>
        {`Released ${physicalReleaseDate}`}
      </div>
    );
  }

  if (sortKey === 'certification' && certification) {
    return (
      <div className={styles.info}>
        {certification}
      </div>
    );
  }

  if (sortKey === 'runtime' && runtime) {
    return (
      <div className={styles.info}>
        {formatRuntime(runtime)}
      </div>
    );
  }

  if (sortKey === 'ratings' && ratings) {
    return (
      <div className={styles.info}>
        <HeartRating
          ratings={ratings}
        />
      </div>
    );
  }

  return null;
}

DiscoverMoviePosterInfo.propTypes = {
  status: PropTypes.string,
  studio: PropTypes.string,
  inCinemas: PropTypes.string,
  certification: PropTypes.string,
  digitalRelease: PropTypes.string,
  physicalRelease: PropTypes.string,
  runtime: PropTypes.number,
  ratings: PropTypes.object.isRequired,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default DiscoverMoviePosterInfo;
