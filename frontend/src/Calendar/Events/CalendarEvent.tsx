import classNames from 'classnames';
import moment from 'moment';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons, kinds } from 'Helpers/Props';
import useMovieFile from 'MovieFile/useMovieFile';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import translate from 'Utilities/String/translate';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

interface CalendarEventProps {
  id: number;
  movieFileId?: number;
  title: string;
  titleSlug: string;
  genres: string[];
  certification?: string;
  date: string;
  inCinemas?: string;
  digitalRelease?: string;
  physicalRelease?: string;
  isAvailable: boolean;
  monitored: boolean;
  hasFile: boolean;
  grabbed?: boolean;
}

function CalendarEvent({
  id,
  movieFileId,
  title,
  titleSlug,
  genres = [],
  certification,
  date,
  inCinemas,
  digitalRelease,
  physicalRelease,
  isAvailable,
  monitored: isMonitored,
  hasFile,
  grabbed,
}: CalendarEventProps) {
  const movieFile = useMovieFile(movieFileId);
  const queueItem = useSelector(createQueueItemSelectorForHook(id));

  const { enableColorImpairedMode } = useSelector(createUISettingsSelector());

  const {
    showMovieInformation,
    showCinemaRelease,
    showDigitalRelease,
    showPhysicalRelease,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useSelector((state: AppState) => state.calendar.options);

  const isDownloading = !!(queueItem || grabbed);
  const statusStyle = getStatusStyle(
    hasFile,
    isDownloading,
    isMonitored,
    isAvailable
  );
  const joinedGenres = genres.slice(0, 2).join(', ');
  const link = `/movie/${titleSlug}`;

  const eventTypes = useMemo(() => {
    const momentDate = moment(date);

    const types = [];

    if (
      showCinemaRelease &&
      inCinemas &&
      momentDate.isSame(moment(inCinemas), 'day')
    ) {
      types.push('Cinemas');
    }

    if (
      showDigitalRelease &&
      digitalRelease &&
      momentDate.isSame(moment(digitalRelease), 'day')
    ) {
      types.push('Digital');
    }

    if (
      showPhysicalRelease &&
      physicalRelease &&
      momentDate.isSame(moment(physicalRelease), 'day')
    ) {
      types.push('Physical');
    }

    return types;
  }, [
    date,
    showCinemaRelease,
    showDigitalRelease,
    showPhysicalRelease,
    inCinemas,
    digitalRelease,
    physicalRelease,
  ]);

  return (
    <div
      className={classNames(
        styles.event,
        styles[statusStyle],
        enableColorImpairedMode && 'colorImpaired',
        fullColorEvents && 'fullColor'
      )}
    >
      <Link className={styles.underlay} to={link} />

      <div className={styles.overlay}>
        <div className={styles.info}>
          <div className={styles.movieTitle}>{title}</div>

          <div
            className={classNames(
              styles.statusContainer,
              fullColorEvents && 'fullColor'
            )}
          >
            {queueItem ? (
              <span className={styles.statusIcon}>
                <CalendarEventQueueDetails {...queueItem} />
              </span>
            ) : null}

            {!queueItem && grabbed ? (
              <Icon
                className={styles.statusIcon}
                name={icons.DOWNLOADING}
                title={translate('MovieIsDownloading')}
              />
            ) : null}

            {showCutoffUnmetIcon &&
            !!movieFile &&
            movieFile.qualityCutoffNotMet ? (
              <Icon
                className={styles.statusIcon}
                name={icons.MOVIE_FILE}
                kind={kinds.WARNING}
                title={translate('QualityCutoffNotMet')}
              />
            ) : null}
          </div>
        </div>

        {showMovieInformation ? (
          <>
            <div className={styles.movieInfo}>
              <div className={styles.genres}>{joinedGenres}</div>
            </div>

            <div className={styles.movieInfo}>
              <div className={styles.eventType}>{eventTypes.join(', ')}</div>

              <div>{certification}</div>
            </div>
          </>
        ) : null}
      </div>
    </div>
  );
}

export default CalendarEvent;
