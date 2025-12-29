import React from 'react';
import Icon, { IconName } from 'Components/Icon';
import formatDate from 'Utilities/Date/formatDate';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import styles from './MovieIndexPoster.css';

interface PosterDateRowProps {
  show: boolean;
  date?: string;
  icon: IconName;
  label: string;
  shortDateFormat: string;
  longDateFormat: string;
  showRelativeDates: boolean;
  timeFormat: string;
}

function PosterDateRow({
  show,
  date,
  icon,
  label,
  shortDateFormat,
  longDateFormat,
  showRelativeDates,
  timeFormat,
}: Readonly<PosterDateRowProps>) {
  if (!show || !date) {
    return null;
  }

  return (
    <div
      className={styles.title}
      title={`${label}: ${formatDate(date, longDateFormat)}`}
    >
      <Icon name={icon} />{' '}
      {getRelativeDate({
        date,
        shortDateFormat,
        showRelativeDates,
        timeFormat,
        timeForToday: false,
      })}
    </div>
  );
}

export default PosterDateRow;
