import React from 'react';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexPosterInfo.css';

interface MovieIndexPosterInfoProps {
  studio?: string;
  showQualityProfile: boolean;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  qualityProfile: any;
  added?: string;
  inCinemas?: string;
  digitalRelease?: string;
  physicalRelease?: string;
  path: string;
  certification: string;
  sizeOnDisk?: number;
  sortKey: string;
  showRelativeDates: boolean;
  showReleaseDate: boolean;
  shortDateFormat: string;
  timeFormat: string;
}

function MovieIndexPosterInfo(props: MovieIndexPosterInfoProps) {
  const {
    studio,
    showQualityProfile,
    qualityProfile,
    added,
    inCinemas,
    digitalRelease,
    physicalRelease,
    path,
    certification,
    sizeOnDisk,
    sortKey,
    showRelativeDates,
    showReleaseDate,
    shortDateFormat,
    timeFormat,
  } = props;

  if (sortKey === 'studio' && studio) {
    return <div className={styles.info}>{studio}</div>;
  }

  if (sortKey === 'qualityProfileId' && !showQualityProfile) {
    return <div className={styles.info}>{qualityProfile.name}</div>;
  }

  if (sortKey === 'added' && added) {
    const addedDate = getRelativeDate(
      added,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false,
      }
    );

    return (
      <div className={styles.info}>
        {translate('Added')}: {addedDate}
      </div>
    );
  }

  if (sortKey === 'inCinemas' && inCinemas && !showReleaseDate) {
    const inCinemasDate = getRelativeDate(
      inCinemas,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false,
      }
    );

    return (
      <div className={styles.info}>
        <Icon name={icons.IN_CINEMAS} /> {inCinemasDate}
      </div>
    );
  }

  if (sortKey === 'digitalRelease' && digitalRelease && !showReleaseDate) {
    const digitalReleaseDate = getRelativeDate(
      digitalRelease,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false,
      }
    );

    return (
      <div className={styles.info}>
        <Icon name={icons.MOVIE_FILE} /> {digitalReleaseDate}
      </div>
    );
  }

  if (sortKey === 'physicalRelease' && physicalRelease && !showReleaseDate) {
    const physicalReleaseDate = getRelativeDate(
      physicalRelease,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false,
      }
    );

    return (
      <div className={styles.info}>
        <Icon name={icons.DISC} /> {physicalReleaseDate}
      </div>
    );
  }

  if (sortKey === 'path') {
    return <div className={styles.info}>{path}</div>;
  }

  if (sortKey === 'sizeOnDisk') {
    return <div className={styles.info}>{formatBytes(sizeOnDisk)}</div>;
  }

  if (sortKey === 'certification') {
    return <div className={styles.info}>{certification}</div>;
  }

  return null;
}

export default MovieIndexPosterInfo;
