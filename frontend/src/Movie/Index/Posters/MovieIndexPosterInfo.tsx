import React from 'react';
import Icon from 'Components/Icon';
import ImdbRating from 'Components/ImdbRating';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import TagListConnector from 'Components/TagListConnector';
import TmdbRating from 'Components/TmdbRating';
import TraktRating from 'Components/TraktRating';
import { icons } from 'Helpers/Props';
import Language from 'Language/Language';
import { Ratings } from 'Movie/Movie';
import QualityProfile from 'typings/QualityProfile';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexPosterInfo.css';

interface MovieIndexPosterInfoProps {
  studio?: string;
  showQualityProfile: boolean;
  qualityProfile?: QualityProfile;
  added?: string;
  year: number;
  inCinemas?: string;
  digitalRelease?: string;
  physicalRelease?: string;
  releaseDate?: string;
  path: string;
  ratings: Ratings;
  certification: string;
  originalTitle: string;
  originalLanguage: Language;
  sizeOnDisk?: number;
  tags: number[];
  sortKey: string;
  showRelativeDates: boolean;
  showCinemaRelease: boolean;
  showDigitalRelease: boolean;
  showPhysicalRelease: boolean;
  showReleaseDate: boolean;
  shortDateFormat: string;
  longDateFormat: string;
  timeFormat: string;
  showTmdbRating: boolean;
  showImdbRating: boolean;
  showRottenTomatoesRating: boolean;
  showTraktRating: boolean;
  showTags: boolean;
}

function MovieIndexPosterInfo(props: MovieIndexPosterInfoProps) {
  const {
    studio,
    showQualityProfile,
    qualityProfile,
    added,
    year,
    inCinemas,
    digitalRelease,
    physicalRelease,
    releaseDate,
    path,
    ratings,
    certification,
    originalTitle,
    originalLanguage,
    sizeOnDisk = 0,
    tags = [],
    sortKey,
    showRelativeDates,
    showCinemaRelease,
    showDigitalRelease,
    showPhysicalRelease,
    showReleaseDate,
    shortDateFormat,
    longDateFormat,
    timeFormat,
    showTmdbRating,
    showImdbRating,
    showRottenTomatoesRating,
    showTraktRating,
    showTags,
  } = props;

  if (sortKey === 'studio' && studio) {
    return (
      <div className={styles.info} title={translate('Studio')}>
        {studio}
      </div>
    );
  }

  if (
    sortKey === 'qualityProfileId' &&
    !showQualityProfile &&
    !!qualityProfile?.name
  ) {
    return (
      <div className={styles.info} title={translate('QualityProfile')}>
        {qualityProfile.name}
      </div>
    );
  }

  if (sortKey === 'added' && added) {
    const addedDate = getRelativeDate({
      date: added,
      shortDateFormat,
      showRelativeDates,
      timeFormat,
      timeForToday: false,
    });

    return (
      <div
        className={styles.info}
        title={formatDateTime(added, longDateFormat, timeFormat)}
      >
        {translate('Added')}: {addedDate}
      </div>
    );
  }

  if (sortKey === 'year' && year) {
    return (
      <div className={styles.info} title={translate('Year')}>
        {year}
      </div>
    );
  }

  if (sortKey === 'inCinemas' && inCinemas && !showCinemaRelease) {
    const inCinemasDate = getRelativeDate({
      date: inCinemas,
      shortDateFormat,
      showRelativeDates,
      timeFormat,
      timeForToday: false,
    });

    return (
      <div className={styles.info} title={translate('InCinemas')}>
        <Icon name={icons.IN_CINEMAS} /> {inCinemasDate}
      </div>
    );
  }

  if (sortKey === 'digitalRelease' && digitalRelease && !showDigitalRelease) {
    const digitalReleaseDate = getRelativeDate({
      date: digitalRelease,
      shortDateFormat,
      showRelativeDates,
      timeFormat,
      timeForToday: false,
    });

    return (
      <div className={styles.info} title={translate('DigitalRelease')}>
        <Icon name={icons.MOVIE_FILE} /> {digitalReleaseDate}
      </div>
    );
  }

  if (
    sortKey === 'physicalRelease' &&
    physicalRelease &&
    !showPhysicalRelease
  ) {
    const physicalReleaseDate = getRelativeDate({
      date: physicalRelease,
      shortDateFormat,
      showRelativeDates,
      timeFormat,
      timeForToday: false,
    });

    return (
      <div className={styles.info} title={translate('PhysicalRelease')}>
        <Icon name={icons.DISC} /> {physicalReleaseDate}
      </div>
    );
  }

  if (sortKey === 'releaseDate' && releaseDate && !showReleaseDate) {
    return (
      <div className={styles.info} title={translate('ReleaseDate')}>
        <Icon name={icons.CALENDAR} />{' '}
        {getRelativeDate({
          date: releaseDate,
          shortDateFormat,
          showRelativeDates,
          timeFormat,
          timeForToday: false,
        })}
      </div>
    );
  }

  if (!showTmdbRating && sortKey === 'tmdbRating' && !!ratings.tmdb) {
    return (
      <div className={styles.info}>
        <TmdbRating ratings={ratings} iconSize={12} />
      </div>
    );
  }

  if (!showImdbRating && sortKey === 'imdbRating' && !!ratings.imdb) {
    return (
      <div className={styles.info}>
        <ImdbRating ratings={ratings} iconSize={12} />
      </div>
    );
  }

  if (
    !showRottenTomatoesRating &&
    sortKey === 'rottenTomatoesRating' &&
    !!ratings.rottenTomatoes
  ) {
    return (
      <div className={styles.info}>
        <RottenTomatoRating ratings={ratings} iconSize={12} />
      </div>
    );
  }

  if (!showTraktRating && sortKey === 'traktRating' && !!ratings.trakt) {
    return (
      <div className={styles.info}>
        <TraktRating ratings={ratings} iconSize={12} />
      </div>
    );
  }

  if (!showTags && sortKey === 'tags' && tags.length) {
    return (
      <div className={styles.tags}>
        <div className={styles.tagsList}>
          <TagListConnector tags={tags} />
        </div>
      </div>
    );
  }

  if (sortKey === 'path') {
    return (
      <div className={styles.info} title={translate('Path')}>
        {path}
      </div>
    );
  }

  if (sortKey === 'sizeOnDisk') {
    return (
      <div className={styles.info} title={translate('SizeOnDisk')}>
        {formatBytes(sizeOnDisk)}
      </div>
    );
  }

  if (sortKey === 'certification') {
    return <div className={styles.info}>{certification}</div>;
  }

  if (sortKey === 'originalTitle' && originalTitle) {
    return (
      <div className={styles.title} title={originalTitle}>
        {originalTitle}
      </div>
    );
  }

  if (sortKey === 'originalLanguage' && originalLanguage) {
    return (
      <div className={styles.info} title={translate('OriginalLanguage')}>
        {originalLanguage.name}
      </div>
    );
  }

  return null;
}

export default MovieIndexPosterInfo;
