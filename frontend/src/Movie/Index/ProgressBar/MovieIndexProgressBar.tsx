import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import getStatusStyle from 'Utilities/Movie/getStatusStyle';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexProgressBar.css';

interface MovieIndexProgressBarProps {
  monitored: boolean;
  status: string;
  hasFile: boolean;
  isAvailable: boolean;
  posterWidth: number;
  detailedProgressBar: boolean;
  bottomRadius: boolean;
  queueStatus: string;
  queueState: string;
}

function MovieIndexProgressBar(props: MovieIndexProgressBarProps) {
  const {
    monitored,
    status,
    hasFile,
    isAvailable,
    posterWidth,
    detailedProgressBar,
    bottomRadius,
    queueStatus,
    queueState,
  } = props;

  const progress = 100;
  const queueStatusText = getQueueStatusText(queueStatus, queueState);
  let movieStatus = status === 'released' && hasFile ? 'downloaded' : status;

  if (movieStatus === 'deleted') {
    movieStatus = 'Missing';

    if (hasFile) {
      movieStatus = 'Downloaded';
    }
  } else if (hasFile) {
    movieStatus = 'Downloaded';
  } else if (isAvailable && !hasFile) {
    movieStatus = 'Missing';
  } else {
    movieStatus = 'NotAvailable';
  }

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={
        bottomRadius ? styles.progressRadius : styles.progress
      }
      progress={progress}
      kind={getStatusStyle(
        status,
        monitored,
        hasFile,
        isAvailable,
        'kinds',
        queueStatusText
      )}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={posterWidth}
      text={queueStatusText ? queueStatusText : translate(movieStatus)}
    />
  );
}

export default MovieIndexProgressBar;
