import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
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
    showReleaseDate,
    showSearchAction,
  } = useSelector(selectPosterOptions);

  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

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
    inCinemas,
    physicalRelease,
    digitalRelease,
    path,
    movieFile,
    certification,
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

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer}>
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

      {showTitle ? <div className={styles.title}>{title}</div> : null}

      {showMonitored ? (
        <div className={styles.title}>
          {monitored ? translate('monitored') : translate('unmonitored')}
        </div>
      ) : null}

      {showQualityProfile ? (
        <div className={styles.title}>{qualityProfile.name}</div>
      ) : null}

      <MovieIndexPosterInfo
        qualityProfile={qualityProfile}
        showQualityProfile={showQualityProfile}
        showReleaseDate={showReleaseDate}
        showRelativeDates={showRelativeDates}
        shortDateFormat={shortDateFormat}
        timeFormat={timeFormat}
        inCinemas={inCinemas}
        physicalRelease={physicalRelease}
        digitalRelease={digitalRelease}
        sortKey={sortKey}
        path={path}
        certification={certification}
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
