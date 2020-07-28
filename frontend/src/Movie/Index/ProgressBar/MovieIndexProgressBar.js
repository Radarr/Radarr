import PropTypes from 'prop-types';
import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import getProgressBarKind from 'Utilities/Movie/getProgressBarKind';
import styles from './MovieIndexProgressBar.css';

function MovieIndexProgressBar(props) {
  const {
    monitored,
    status,
    hasFile,
    posterWidth,
    detailedProgressBar
  } = props;

  const progress = 100;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={styles.progress}
      progress={progress}
      kind={getProgressBarKind(status, monitored, hasFile)}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={false} // Hide until we have multi version support
      width={posterWidth}
    />
  );
}

MovieIndexProgressBar.propTypes = {
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired
};

export default MovieIndexProgressBar;
