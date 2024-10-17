import React from 'react';
import { useSelector } from 'react-redux';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import createMovieQueueItemsDetailsSelector, {
  MovieQueueDetails,
} from 'Movie/Index/createMovieQueueDetailsSelector';
import { MovieStatus } from 'Movie/Movie';
import { MovieFile } from 'MovieFile/MovieFile';
import getProgressBarKind from 'Utilities/Movie/getProgressBarKind';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexProgressBar.css';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';

interface MovieIndexProgressBarProps {
  movieId: number;
  movieFile: MovieFile;
  monitored: boolean;
  status: MovieStatus;
  hasFile: boolean;
  isAvailable: boolean;
  isAvailableDate: string;
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
    isAvailableDate,
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

  let movieStatus = translate('NotAvailable');
  if (hasFile) {
    movieStatus = movieFile?.quality?.quality.name ?? translate('Downloaded');
  } else if (status === 'deleted') {
    movieStatus = translate('Deleted');
  } else if (isAvailable && !hasFile) {
    movieStatus = translate('Missing');
  }

  const attachedClassName = bottomRadius
    ? styles.progressRadius
    : styles.progress;
  const containerClassName = isStandAlone ? undefined : attachedClassName;


  const uiSettings = useSelector(createUISettingsSelector());
  const { showRelativeDates, shortDateFormat, timeFormat } =
      uiSettings;
  const DateConsideredAvailable = getRelativeDate(
    isAvailableDate,
    shortDateFormat,
    showRelativeDates,
    {
      timeFormat,
      timeForToday: false
    }
  );
  
  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={containerClassName}
      progress={progress}
      kind={getProgressBarKind(
        status,
        monitored,
        hasFile,
        isAvailable,
        queueDetails.count > 0
      )}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={width}
      text={queueStatusText ? queueStatusText : movieStatus}
      title={movieStatus=='NotAvailable' ? DateConsideredAvailable : ''}
    />
  );
}

export default MovieIndexProgressBar;
