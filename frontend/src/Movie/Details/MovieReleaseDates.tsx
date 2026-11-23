import React from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import { icons } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatDate from 'Utilities/Date/formatDate';
import getLocalCalendarDate from 'Utilities/Date/getLocalCalendarDate';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import styles from './MovieReleaseDates.css';

type MovieReleaseDatesProps = Pick<
  Movie,
  'tmdbId' | 'inCinemas' | 'digitalRelease' | 'physicalRelease'
>;

function MovieReleaseDates({
  tmdbId,
  inCinemas,
  digitalRelease,
  physicalRelease,
}: MovieReleaseDatesProps) {
  const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());

  if (!inCinemas && !physicalRelease && !digitalRelease) {
    return (
      <div>
        <div className={styles.dateIcon}>
          <Icon name={icons.MISSING} />
        </div>

        <InlineMarkdown
          data={translate('NoMovieReleaseDatesAvailable', {
            url: `https://www.themoviedb.org/movie/${tmdbId}`,
          })}
        />
      </div>
    );
  }

  return (
    <>
      {inCinemas ? (
        <div
          title={`${translate('InCinemas')}: ${formatDate(
            getLocalCalendarDate(inCinemas),
            longDateFormat
          )}`}
        >
          <div className={styles.dateIcon}>
            <Icon name={icons.IN_CINEMAS} />
          </div>

          {getRelativeDate({
            date: getLocalCalendarDate(inCinemas),
            shortDateFormat,
            showRelativeDates,
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}

      {digitalRelease ? (
        <div
          title={`${translate('DigitalRelease')}: ${formatDate(
            getLocalCalendarDate(digitalRelease),
            longDateFormat
          )}`}
        >
          <div className={styles.dateIcon}>
            <Icon name={icons.MOVIE_FILE} />
          </div>

          {getRelativeDate({
            date: getLocalCalendarDate(digitalRelease),
            shortDateFormat,
            showRelativeDates,
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}

      {physicalRelease ? (
        <div
          title={`${translate('PhysicalRelease')}: ${formatDate(
            getLocalCalendarDate(physicalRelease),
            longDateFormat
          )}`}
        >
          <div className={styles.dateIcon}>
            <Icon name={icons.DISC} />
          </div>

          {getRelativeDate({
            date: getLocalCalendarDate(physicalRelease),
            shortDateFormat,
            showRelativeDates,
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}
    </>
  );
}

export default MovieReleaseDates;
