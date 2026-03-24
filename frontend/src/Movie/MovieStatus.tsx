import React from 'react';
import { useSelector } from 'react-redux';
import QueueDetails from 'Activity/Queue/QueueDetails';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import { icons, kinds, sizes } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import useMovie, { MovieEntity } from 'Movie/useMovie';
import useMovieFile from 'MovieFile/useMovieFile';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import translate from 'Utilities/String/translate';
import MovieQuality from './MovieQuality';
import styles from './MovieStatus.css';

interface MovieStatusProps {
  movieId: number;
  movieEntity?: MovieEntity;
  movieFileId: number | undefined;
}

function MovieStatus({ movieId, movieFileId }: MovieStatusProps) {
  const {
    isAvailable,
    monitored,
    grabbed = false,
  } = useMovie(movieId) as Movie;

  const queueItem = useSelector(createQueueItemSelectorForHook(movieId));
  const movieFile = useMovieFile(movieFileId);

  const hasMovieFile = !!movieFile;
  const isQueued = !!queueItem;

  if (isQueued) {
    const { sizeleft, size } = queueItem;

    const progress = size ? 100 - (sizeleft / size) * 100 : 0;

    return (
      <div className={styles.center}>
        <QueueDetails
          {...queueItem}
          progressBar={
            <ProgressBar
              progress={progress}
              kind={kinds.PURPLE}
              size={sizes.MEDIUM}
            />
          }
        />
      </div>
    );
  }

  if (grabbed) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.DOWNLOADING}
          title={translate('MovieIsDownloading')}
        />
      </div>
    );
  }

  if (hasMovieFile) {
    const quality = movieFile.quality;
    const isCutoffNotMet = movieFile.qualityCutoffNotMet;

    return (
      <div className={styles.center}>
        <MovieQuality
          quality={quality}
          size={movieFile.size}
          isCutoffNotMet={isCutoffNotMet}
          title={translate('MovieDownloaded')}
        />
      </div>
    );
  }

  if (!monitored) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.UNMONITORED}
          kind={kinds.DISABLED}
          title={translate('MovieIsNotMonitored')}
        />
      </div>
    );
  }

  if (isAvailable) {
    return (
      <div className={styles.center}>
        <Icon name={icons.MISSING} title={translate('MovieMissingFromDisk')} />
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Icon name={icons.NOT_AIRED} title={translate('MovieIsNotAvailable')} />
    </div>
  );
}

export default MovieStatus;
