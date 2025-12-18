import React from 'react';
import Icon from 'Components/Icon';
import { IconDefinition } from 'Helpers/Props/icons';
import formatDate from 'Utilities/Date/formatDate';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import styles from './MovieIndexPoster.css';

interface PosterDateRowProps {
  show: boolean;
  date?: string;
  icon: IconDefinition;
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
    <div className={styles.title} title={`${label}: ${formatDate(date, longDateFormat)}`}>
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
