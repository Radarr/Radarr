import classNames from 'classnames';
import moment from 'moment';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import CalendarEventQueueDetails from 'Calendar/Events/CalendarEventQueueDetails';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons, kinds } from 'Helpers/Props';
import useMovieFile from 'MovieFile/useMovieFile';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getLocalCalendarDate from 'Utilities/Date/getLocalCalendarDate';
import translate from 'Utilities/String/translate';
import styles from './AgendaEvent.css';

interface AgendaEventProps {
  id: number;
  movieFileId: number;
  title: string;
  titleSlug: string;
  genres: string[];
  inCinemas?: string;
  digitalRelease?: string;
  physicalRelease?: string;
  sortDate: moment.Moment;
  isAvailable: boolean;
  monitored: boolean;
  hasFile: boolean;
  grabbed?: boolean;
  showDate: boolean;
}

function AgendaEvent({
  id,
  movieFileId,
  title,
  titleSlug,
  genres = [],
  inCinemas,
  digitalRelease,
  physicalRelease,
  sortDate,
  isAvailable,
  monitored: isMonitored,
  hasFile,
  grabbed,
  showDate,
}: AgendaEventProps) {
  const movieFile = useMovieFile(movieFileId);
  const queueItem = useSelector(createQueueItemSelectorForHook(id));
  const { longDateFormat, enableColorImpairedMode } = useSelector(
    createUISettingsSelector()
  );

  const { showMovieInformation, showCutoffUnmetIcon } = useSelector(
    (state: AppState) => state.calendar.options
  );

  const { eventDate, eventTitle, releaseIcon } = useMemo(() => {
    const localPhysicalRelease = getLocalCalendarDate(physicalRelease);
    const localDigitalRelease = getLocalCalendarDate(digitalRelease);
    const localInCinemas = getLocalCalendarDate(inCinemas);

    if (localPhysicalRelease && sortDate.isSame(localPhysicalRelease, 'day')) {
      return {
        eventDate: localPhysicalRelease,
        eventTitle: translate('PhysicalRelease'),
        releaseIcon: icons.DISC,
      };
    }

    if (localDigitalRelease && sortDate.isSame(localDigitalRelease, 'day')) {
      return {
        eventDate: localDigitalRelease,
        eventTitle: translate('DigitalRelease'),
        releaseIcon: icons.MOVIE_FILE,
      };
    }

    if (localInCinemas && sortDate.isSame(localInCinemas, 'day')) {
      return {
        eventDate: localInCinemas,
        eventTitle: translate('InCinemas'),
        releaseIcon: icons.IN_CINEMAS,
      };
    }

    return {
      eventDate: null,
      eventTitle: null,
      releaseIcon: null,
    };
  }, [inCinemas, digitalRelease, physicalRelease, sortDate]);

  const downloading = !!(queueItem || grabbed);
  const statusStyle = getStatusStyle(
    hasFile,
    downloading,
    isMonitored,
    isAvailable
  );
  const joinedGenres = genres.slice(0, 2).join(', ');
  const link = `/movie/${titleSlug}`;

  return (
    <div className={styles.event}>
      <Link className={styles.underlay} to={link} />

      <div className={styles.overlay}>
        <div className={styles.date}>
          {showDate && eventDate
            ? moment(eventDate).format(longDateFormat)
            : null}
        </div>

        <div className={styles.releaseIcon}>
          {releaseIcon ? (
            <Icon name={releaseIcon} kind={kinds.DEFAULT} title={eventTitle} />
          ) : null}
        </div>

        <div
          className={classNames(
            styles.eventWrapper,
            styles[statusStyle],
            enableColorImpairedMode && 'colorImpaired'
          )}
        >
          <div className={styles.movieTitle}>{title}</div>

          {showMovieInformation ? (
            <div className={styles.genres}>{joinedGenres}</div>
          ) : null}

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

          {showCutoffUnmetIcon && movieFile && movieFile.qualityCutoffNotMet ? (
            <Icon
              className={styles.statusIcon}
              name={icons.MOVIE_FILE}
              kind={kinds.WARNING}
              title={translate('QualityCutoffNotMet')}
            />
          ) : null}
        </div>
      </div>
    </div>
  );
}

export default AgendaEvent;
