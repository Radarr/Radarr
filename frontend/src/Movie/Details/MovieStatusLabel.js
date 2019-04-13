import PropTypes from 'prop-types';
import React from 'react';
import moment from 'moment';
import styles from './MovieStatusLabel.css';

function getMovieStatus(hasFile, isMonitored, inCinemas) {
  const currentTime = moment();

  if (hasFile) {
    return 'Downloaded';
  }

  if (!isMonitored) {
    return 'Unmonitored';
  }

  if (inCinemas.isBefore(currentTime) && !hasFile) {
    return 'Missing';
  }

  return 'Unaired';
}

function MovieStatusLabel(props) {
  const {
    hasMovieFiles,
    monitored,
    inCinemas
  } = props;

  const status = getMovieStatus(hasMovieFiles, monitored, moment(inCinemas));

  return (
    <span
      className={styles[status.toLowerCase()]}
    >
      {status}
    </span>
  );
}

MovieStatusLabel.propTypes = {
  hasMovieFiles: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
  inCinemas: PropTypes.string.isRequired
};

MovieStatusLabel.defaultProps = {
  title: ''
};

export default MovieStatusLabel;
