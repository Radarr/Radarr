import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Icon from 'Components/Icon';
import ImdbRating from 'Components/ImdbRating';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import TmdbRating from 'Components/TmdbRating';
import Popover from 'Components/Tooltip/Popover';
import { icons } from 'Helpers/Props';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import MovieIndexProgressBar from 'Movie/Index/ProgressBar/MovieIndexProgressBar';
import MovieIndexPosterSelect from 'Movie/Index/Select/MovieIndexPosterSelect';
import MoviePoster from 'Movie/MoviePoster';
import { executeCommand } from 'Store/Actions/commandActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import createMovieIndexItemSelector from '../createMovieIndexItemSelector';
import MovieIndexPosterInfo from './MovieIndexPosterInfo';
import selectPosterOptions from './selectPosterOptions';
import styles from './MovieIndexPoster.css';

interface MovieIndexPosterProps {
  movieId: number;
  sortKey: string;
  isSelectMode: boolean;
  posterWidth: number;
  posterHeight: number;
}

function MovieIndexPoster(props: MovieIndexPosterProps) {
  const { movieId, sortKey, isSelectMode, posterWidth, posterHeight } = props;

  const { movie, qualityProfile, isRefreshingMovie, isSearchingMovie } =
    useSelector(createMovieIndexItemSelector(props.movieId));

  const {
    detailedProgressBar,
    showTitle,
    showMonitored,
    showQualityProfile,
    showCinemaRelease,
    showReleaseDate,
    showTmdbRating,
    showImdbRating,
    showRottenTomatoesRating,
    showSearchAction,
  } = useSelector(selectPosterOptions);

  const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());

  const {
    title,
    monitored,
    status,
    images,
    tmdbId,
    imdbId,
    youTubeTrailerId,
    hasFile,
    isAvailable,
    studio,
    added,
    year,
    inCinemas,
    physicalRelease,
    digitalRelease,
    path,
    movieFile,
    ratings,
    sizeOnDisk,
    certification,
    originalTitle,
    originalLanguage,
  } = movie;

  const dispatch = useDispatch();
  const [hasPosterError, setHasPosterError] = useState(false);
  const [isEditMovieModalOpen, setIsEditMovieModalOpen] = useState(false);
  const [isDeleteMovieModalOpen, setIsDeleteMovieModalOpen] = useState(false);

  const onRefreshPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_MOVIE,
        movieIds: [movieId],
      })
    );
  }, [movieId, dispatch]);

  const onSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: MOVIE_SEARCH,
        movieIds: [movieId],
      })
    );
  }, [movieId, dispatch]);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

  const onEditMoviePress = useCallback(() => {
    setIsEditMovieModalOpen(true);
  }, [setIsEditMovieModalOpen]);

  const onEditMovieModalClose = useCallback(() => {
    setIsEditMovieModalOpen(false);
  }, [setIsEditMovieModalOpen]);

  const onDeleteMoviePress = useCallback(() => {
    setIsEditMovieModalOpen(false);
    setIsDeleteMovieModalOpen(true);
  }, [setIsDeleteMovieModalOpen]);

  const onDeleteMovieModalClose = useCallback(() => {
    setIsDeleteMovieModalOpen(false);
  }, [setIsDeleteMovieModalOpen]);

  const link = `/movie/${tmdbId}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  let releaseDate = '';
  let releaseDateType = '';
  if (physicalRelease && digitalRelease) {
    releaseDate =
      physicalRelease < digitalRelease ? physicalRelease : digitalRelease;
    releaseDateType = physicalRelease < digitalRelease ? 'Released' : 'Digital';
  } else if (physicalRelease && !digitalRelease) {
    releaseDate = physicalRelease;
    releaseDateType = 'Released';
  } else if (digitalRelease && !physicalRelease) {
    releaseDate = digitalRelease;
    releaseDateType = 'Digital';
  }

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={title}>
        {isSelectMode ? <MovieIndexPosterSelect movieId={movieId} /> : null}

        <Label className={styles.controls}>
          <SpinnerIconButton
            name={icons.REFRESH}
            title={translate('RefreshMovie')}
            isSpinning={isRefreshingMovie}
            onPress={onRefreshPress}
          />

          {showSearchAction ? (
            <SpinnerIconButton
              className={styles.action}
              name={icons.SEARCH}
              title={translate('SearchForMovie')}
              isSpinning={isSearchingMovie}
              onPress={onSearchPress}
            />
          ) : null}

          <IconButton
            name={icons.EDIT}
            title={translate('EditMovie')}
            onPress={onEditMoviePress}
          />

          <span className={styles.externalLinks}>
            <Popover
              anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
              title={translate('Links')}
              body={
                <MovieDetailsLinks
                  tmdbId={tmdbId}
                  imdbId={imdbId}
                  youTubeTrailerId={youTubeTrailerId}
                />
              }
            />
          </span>
        </Label>

        <Link className={styles.link} style={elementStyle} to={link}>
          <MoviePoster
            style={elementStyle}
            images={images}
            size={250}
            lazy={false}
            overflow={true}
            onError={onPosterLoadError}
            onLoad={onPosterLoad}
          />

          {hasPosterError ? (
            <div className={styles.overlayTitle}>{title}</div>
          ) : null}
        </Link>
      </div>

      <MovieIndexProgressBar
        movieId={movieId}
        movieFile={movieFile}
        monitored={monitored}
        hasFile={hasFile}
        isAvailable={isAvailable}
        status={status}
        width={posterWidth}
        detailedProgressBar={detailedProgressBar}
        bottomRadius={false}
      />

      {showTitle ? (
        <div className={styles.title} title={title}>
          {title}
        </div>
      ) : null}

      {showMonitored ? (
        <div className={styles.title}>
          {monitored ? translate('Monitored') : translate('Unmonitored')}
        </div>
      ) : null}

      {showQualityProfile && !!qualityProfile?.name ? (
        <div className={styles.title} title={translate('QualityProfile')}>
          {qualityProfile.name}
        </div>
      ) : null}

      {showCinemaRelease && inCinemas ? (
        <div className={styles.title} title={translate('InCinemas')}>
          <Icon name={icons.IN_CINEMAS} />{' '}
          {getRelativeDate(inCinemas, shortDateFormat, showRelativeDates, {
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}

      {showReleaseDate && releaseDate ? (
        <div className={styles.title}>
          <Icon
            name={releaseDateType === 'Digital' ? icons.MOVIE_FILE : icons.DISC}
          />{' '}
          {getRelativeDate(releaseDate, shortDateFormat, showRelativeDates, {
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}

      {showTmdbRating && !!ratings.tmdb ? (
        <div className={styles.title}>
          <TmdbRating ratings={ratings} iconSize={12} />
        </div>
      ) : null}

      {showImdbRating && !!ratings.imdb ? (
        <div className={styles.title}>
          <ImdbRating ratings={ratings} iconSize={12} />
        </div>
      ) : null}

      {showRottenTomatoesRating && !!ratings.rottenTomatoes ? (
        <div className={styles.title}>
          <RottenTomatoRating ratings={ratings} iconSize={12} />
        </div>
      ) : null}

      <MovieIndexPosterInfo
        studio={studio}
        qualityProfile={qualityProfile}
        added={added}
        year={year}
        showQualityProfile={showQualityProfile}
        showCinemaRelease={showCinemaRelease}
        showReleaseDate={showReleaseDate}
        showRelativeDates={showRelativeDates}
        shortDateFormat={shortDateFormat}
        longDateFormat={longDateFormat}
        timeFormat={timeFormat}
        inCinemas={inCinemas}
        physicalRelease={physicalRelease}
        digitalRelease={digitalRelease}
        ratings={ratings}
        sizeOnDisk={sizeOnDisk}
        sortKey={sortKey}
        path={path}
        certification={certification}
        originalTitle={originalTitle}
        originalLanguage={originalLanguage}
        showTmdbRating={showTmdbRating}
        showImdbRating={showImdbRating}
        showRottenTomatoesRating={showRottenTomatoesRating}
      />

      <EditMovieModalConnector
        isOpen={isEditMovieModalOpen}
        movieId={movieId}
        onModalClose={onEditMovieModalClose}
        onDeleteMoviePress={onDeleteMoviePress}
      />

      <DeleteMovieModal
        isOpen={isDeleteMovieModalOpen}
        movieId={movieId}
        onModalClose={onDeleteMovieModalClose}
      />
    </div>
  );
}

export default MovieIndexPoster;
