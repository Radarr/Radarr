import React from 'react';
import { useSelector } from 'react-redux';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import createMovieQueueItemsDetailsSelector, {
  MovieQueueDetails,
} from 'Movie/Index/createMovieQueueDetailsSelector';
import { MovieFile } from 'MovieFile/MovieFile';
import getStatusStyle from 'Utilities/Movie/getStatusStyle';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexProgressBar.css';

interface MovieIndexProgressBarProps {
  movieId: number;
  movieFile: MovieFile;
  monitored: boolean;
  status: string;
  hasFile: boolean;
  isAvailable: boolean;
  width: number;
  detailedProgressBar: boolean;
  bottomRadius?: boolean;
  isStandAlone?: boolean;
}

function MovieIndexProgressBar(props: MovieIndexProgressBarProps) {
  const {
    movieId,
    movieFile,
    monitored,
    status,
    hasFile,
    isAvailable,
    width,
    detailedProgressBar,
    bottomRadius,
    isStandAlone,
  } = props;

  const queueDetails: MovieQueueDetails = useSelector(
    createMovieQueueItemsDetailsSelector(movieId)
  );

  const progress = 100;
  const queueStatusText =
    queueDetails.count > 0 ? translate('Downloading') : null;
  let movieStatus = status === 'released' && hasFile ? 'downloaded' : status;

  if (movieStatus === 'deleted') {
    movieStatus = translate('Missing');

    if (hasFile) {
      movieStatus = movieFile?.quality?.quality.name ?? translate('Downloaded');
    }
  } else if (hasFile) {
    movieStatus = movieFile?.quality?.quality.name ?? translate('Downloaded');
  } else if (isAvailable && !hasFile) {
    movieStatus = translate('Missing');
  } else {
    movieStatus = translate('NotAvailable');
  }

  const attachedClassName = bottomRadius
    ? styles.progressRadius
    : styles.progress;
  const containerClassName = isStandAlone ? undefined : attachedClassName;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={containerClassName}
      progress={progress}
      kind={getStatusStyle(
        status,
        monitored,
        hasFile,
        isAvailable,
        'kinds',
        queueDetails.count > 0
      )}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={width}
      text={queueStatusText ? queueStatusText : movieStatus}
    />
  );
}

export default MovieIndexProgressBar;
