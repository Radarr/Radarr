import PropTypes from 'prop-types';
import React from 'react';
import getProgressBarKind from 'Utilities/Author/getProgressBarKind';
import { sizes } from 'Helpers/Props';
import ProgressBar from 'Components/ProgressBar';
import styles from './AuthorIndexProgressBar.css';

function AuthorIndexProgressBar(props) {
  const {
    monitored,
    status,
    bookCount,
    bookFileCount,
    totalBookCount,
    posterWidth,
    detailedProgressBar
  } = props;

  const progress = bookCount ? bookFileCount / bookCount * 100 : 100;
  const text = `${bookFileCount} / ${bookCount}`;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={styles.progress}
      progress={progress}
      kind={getProgressBarKind(status, monitored, progress)}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      text={text}
      title={`${bookFileCount} / ${bookCount} (Total: ${totalBookCount})`}
      width={posterWidth}
    />
  );
}

AuthorIndexProgressBar.propTypes = {
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  bookCount: PropTypes.number.isRequired,
  bookFileCount: PropTypes.number.isRequired,
  totalBookCount: PropTypes.number.isRequired,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired
};

export default AuthorIndexProgressBar;
