import React from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatDate from 'Utilities/Date/formatDate';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import styles from './MovieReleaseDates.css';

type MovieReleaseDatesProps = Pick<
  Movie,
  'inCinemas' | 'digitalRelease' | 'physicalRelease'
>;

function MovieReleaseDates({
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
        {translate('NoMovieReleaseDatesAvailable')}
      </div>
    );
  }

  return (
    <>
      {inCinemas ? (
        <div
          title={`${translate('InCinemas')}: ${formatDate(
            inCinemas,
            longDateFormat
          )}`}
        >
          <div className={styles.dateIcon}>
            <Icon name={icons.IN_CINEMAS} />
          </div>

          {getRelativeDate({
            date: inCinemas,
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
            digitalRelease,
            longDateFormat
          )}`}
        >
          <div className={styles.dateIcon}>
            <Icon name={icons.MOVIE_FILE} />
          </div>

          {getRelativeDate({
            date: digitalRelease,
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
            physicalRelease,
            longDateFormat
          )}`}
        >
          <div className={styles.dateIcon}>
            <Icon name={icons.DISC} />
          </div>

          {getRelativeDate({
            date: physicalRelease,
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
