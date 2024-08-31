import React from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import styles from './MovieReleaseDates.css';

interface MovieReleaseDatesProps {
  inCinemas?: string;
  digitalRelease?: string;
  physicalRelease?: string;
}

function MovieReleaseDates(props: MovieReleaseDatesProps) {
  const { inCinemas, digitalRelease, physicalRelease } = props;

  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

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
        <div title={translate('InCinemas')}>
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
        <div title={translate('DigitalRelease')}>
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
        <div title={translate('PhysicalRelease')}>
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
