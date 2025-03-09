import React from 'react';
import { useSelector } from 'react-redux';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { MovieStatus } from 'Movie/Movie';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import Queue from 'typings/Queue';
import getQueueStatusText from 'Utilities/Movie/getQueueStatusText';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import styles from './MovieStatusLabel.css';

function getMovieStatus(
  status: MovieStatus,
  isMonitored: boolean,
  isAvailable: boolean,
  hasFiles: boolean,
  queueItem: Queue | null = null
) {
  if (queueItem) {
    const queueStatus = queueItem.status;
    const queueState = queueItem.trackedDownloadStatus;
    const queueStatusText = getQueueStatusText(queueStatus, queueState);

    if (queueStatusText) {
      return queueStatusText;
    }
  }

  if (hasFiles && !isMonitored) {
    return 'availNotMonitored';
  }

  if (hasFiles) {
    return 'ended';
  }

  if (status === 'deleted') {
    return 'deleted';
  }

  if (isAvailable && !isMonitored && !hasFiles) {
    return 'missingUnmonitored';
  }

  if (isAvailable && !hasFiles) {
    return 'missingMonitored';
  }

  return 'continuing';
}

interface MovieStatusLabelProps {
  movieId: number;
  monitored: boolean;
  isAvailable: boolean;
  hasMovieFiles: boolean;
  status: MovieStatus;
  useLabel?: boolean;
}

function MovieStatusLabel({
  movieId,
  monitored,
  isAvailable,
  hasMovieFiles,
  status,
  useLabel = false,
}: MovieStatusLabelProps) {
  const queueItem = useSelector(createQueueItemSelectorForHook(movieId));

  let movieStatus = getMovieStatus(
    status,
    monitored,
    isAvailable,
    hasMovieFiles,
    queueItem
  );

  let statusClass = movieStatus;

  if (movieStatus === 'availNotMonitored' || movieStatus === 'ended') {
    movieStatus = 'downloaded';
  } else if (
    movieStatus === 'missingMonitored' ||
    movieStatus === 'missingUnmonitored'
  ) {
    movieStatus = 'missing';
  } else if (movieStatus === 'continuing') {
    movieStatus = 'notAvailable';
  }

  if (queueItem) {
    statusClass = 'queue';
  }

  if (useLabel) {
    let kind: Kind = kinds.SUCCESS;

    switch (statusClass) {
      case 'queue':
        kind = kinds.QUEUE;
        break;
      case 'missingMonitored':
        kind = kinds.DANGER;
        break;
      case 'continuing':
        kind = kinds.INFO;
        break;
      case 'availNotMonitored':
        kind = kinds.DEFAULT;
        break;
      case 'missingUnmonitored':
        kind = kinds.WARNING;
        break;
      case 'deleted':
        kind = kinds.INVERSE;
        break;
      default:
    }

    return (
      <Label kind={kind} size={sizes.LARGE}>
        {translate(firstCharToUpper(movieStatus))}
      </Label>
    );
  }

  return (
    <span
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      className={styles[statusClass]}
    >
      {translate(firstCharToUpper(movieStatus))}
    </span>
  );
}

export default MovieStatusLabel;
