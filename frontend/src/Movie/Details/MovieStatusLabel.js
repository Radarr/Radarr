import PropTypes from 'prop-types';
import React from 'react';
import styles from './MovieStatusLabel.css';

function getMovieStatus(hasFile, isMonitored, isAvailable) {

  if (hasFile) {
    return 'Downloaded';
  }

  if (!isMonitored) {
    return 'Unmonitored';
  }

  if (isAvailable && !hasFile) {
    return 'Missing';
  }

  return 'Unreleased';
}

function MovieStatusLabel(props) {
  const {
    hasMovieFiles,
    monitored,
    isAvailable
  } = props;

  const status = getMovieStatus(hasMovieFiles, monitored, isAvailable);

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
  isAvailable: PropTypes.bool.isRequired
};

MovieStatusLabel.defaultProps = {
  title: ''
};

export default MovieStatusLabel;
