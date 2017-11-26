import PropTypes from 'prop-types';
import React from 'react';
import getProgressBarKind from 'Utilities/Artist/getProgressBarKind';
import { sizes } from 'Helpers/Props';
import ProgressBar from 'Components/ProgressBar';
import styles from './ArtistIndexProgressBar.css';

function ArtistIndexProgressBar(props) {
  const {
    monitored,
    status,
    trackCount,
    trackFileCount,
    posterWidth,
    detailedProgressBar
  } = props;

  const progress = trackCount ? trackFileCount / trackCount * 100 : 100;
  const text = `${trackFileCount} / ${trackCount}`;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={styles.progress}
      progress={progress}
      kind={getProgressBarKind(status, monitored, progress)}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      text={text}
      title={detailedProgressBar ? null : text}
      width={posterWidth}
    />
  );
}

ArtistIndexProgressBar.propTypes = {
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  trackCount: PropTypes.number.isRequired,
  trackFileCount: PropTypes.number.isRequired,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired
};

export default ArtistIndexProgressBar;
