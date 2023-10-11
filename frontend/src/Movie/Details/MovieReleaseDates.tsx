import React from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import styles from './MovieReleaseDates.css';

interface MovieReleaseDatesProps {
  inCinemas: string;
  physicalRelease: string;
  digitalRelease: string;
}

function MovieReleaseDates(props: MovieReleaseDatesProps) {
  const { inCinemas, physicalRelease, digitalRelease } = props;

  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  return (
    <div>
      {inCinemas ? (
        <div title={translate('InCinemas')}>
          <div className={styles.dateIcon}>
            <Icon name={icons.IN_CINEMAS} />
          </div>
          {getRelativeDate(inCinemas, shortDateFormat, showRelativeDates, {
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
          {getRelativeDate(digitalRelease, shortDateFormat, showRelativeDates, {
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
          {getRelativeDate(
            physicalRelease,
            shortDateFormat,
            showRelativeDates,
            { timeFormat, timeForToday: false }
          )}
        </div>
      ) : null}
    </div>
  );
}

export default MovieReleaseDates;
