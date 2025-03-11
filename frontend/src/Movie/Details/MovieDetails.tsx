import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useHistory } from 'react-router';
import TextTruncate from 'react-text-truncate';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import * as commandNames from 'Commands/commandNames';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import ImdbRating from 'Components/ImdbRating';
import InfoLabel from 'Components/InfoLabel';
import IconButton from 'Components/Link/IconButton';
import Marquee from 'Components/Marquee';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import TmdbRating from 'Components/TmdbRating';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import TraktRating from 'Components/TraktRating';
import useMeasure from 'Helpers/Hooks/useMeasure';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  icons,
  kinds,
  sizes,
  sortDirections,
  tooltipPositions,
} from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import EditMovieModal from 'Movie/Edit/EditMovieModal';
import getMovieStatusDetails from 'Movie/getMovieStatusDetails';
import MovieHistoryModal from 'Movie/History/MovieHistoryModal';
import { Image, Statistics } from 'Movie/Movie';
import MovieCollectionLabel from 'Movie/MovieCollectionLabel';
import MovieGenres from 'Movie/MovieGenres';
import MoviePoster from 'Movie/MoviePoster';
import MovieInteractiveSearchModal from 'Movie/Search/MovieInteractiveSearchModal';
import MovieFileEditorTable from 'MovieFile/Editor/MovieFileEditorTable';
import ExtraFileTable from 'MovieFile/Extras/ExtraFileTable';
import OrganizePreviewModal from 'Organize/OrganizePreviewModal';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  clearExtraFiles,
  fetchExtraFiles,
} from 'Store/Actions/extraFileActions';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import {
  clearMovieCredits,
  fetchMovieCredits,
} from 'Store/Actions/movieCreditsActions';
import {
  clearMovieFiles,
  fetchMovieFiles,
} from 'Store/Actions/movieFileActions';
import {
  clearQueueDetails,
  fetchQueueDetails,
} from 'Store/Actions/queueActions';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchImportListSchema } from 'Store/Actions/Settings/importLists';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import fonts from 'Styles/Variables/fonts';
import sortByProp from 'Utilities/Array/sortByProp';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import formatRuntime from 'Utilities/Date/formatRuntime';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';
import formatBytes from 'Utilities/Number/formatBytes';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import MovieCastPosters from './Credits/Cast/MovieCastPosters';
import MovieCrewPosters from './Credits/Crew/MovieCrewPosters';
import MovieDetailsLinks from './MovieDetailsLinks';
import MovieReleaseDates from './MovieReleaseDates';
import MovieStatusLabel from './MovieStatusLabel';
import MovieTags from './MovieTags';
import MovieTitlesTable from './Titles/MovieTitlesTable';
import styles from './MovieDetails.css';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function getFanartUrl(images: Image[]) {
  const image = images.find((image) => image.coverType === 'fanart');
  return image?.url ?? image?.remoteUrl;
}

function createMovieFilesSelector() {
  return createSelector(
    (state: AppState) => state.movieFiles,
    ({ items, isFetching, isPopulated, error }) => {
      const hasMovieFiles = !!items.length;

      return {
        isMovieFilesFetching: isFetching,
        isMovieFilesPopulated: isPopulated,
        movieFilesError: error,
        hasMovieFiles,
      };
    }
  );
}

function createExtraFilesSelector() {
  return createSelector(
    (state: AppState) => state.extraFiles,
    ({ isFetching, isPopulated, error }) => {
      return {
        isExtraFilesFetching: isFetching,
        isExtraFilesPopulated: isPopulated,
        extraFilesError: error,
      };
    }
  );
}

function createMovieCreditsSelector() {
  return createSelector(
    (state: AppState) => state.movieCredits,
    ({ isFetching, isPopulated, error }) => {
      return {
        isMovieCreditsFetching: isFetching,
        isMovieCreditsPopulated: isPopulated,
        movieCreditsError: error,
      };
    }
  );
}

function createMovieSelector(movieId: number) {
  return createSelector(createAllMoviesSelector(), (allMovies) => {
    const sortedMovies = [...allMovies].sort(sortByProp('sortTitle'));
    const movieIndex = sortedMovies.findIndex((movie) => movie.id === movieId);

    if (movieIndex === -1) {
      return {
        movie: undefined,
        nextMovie: undefined,
        previousMovie: undefined,
      };
    }

    const movie = sortedMovies[movieIndex];
    const nextMovie = sortedMovies[movieIndex + 1] ?? sortedMovies[0];
    const previousMovie =
      sortedMovies[movieIndex - 1] ?? sortedMovies[sortedMovies.length - 1];

    return {
      movie,
      nextMovie: {
        title: nextMovie.title,
        titleSlug: nextMovie.titleSlug,
      },
      previousMovie: {
        title: previousMovie.title,
        titleSlug: previousMovie.titleSlug,
      },
    };
  });
}

interface MovieDetailsProps {
  movieId: number;
}

function MovieDetails({ movieId }: MovieDetailsProps) {
  const dispatch = useDispatch();
  const history = useHistory();

  const { movie, nextMovie, previousMovie } = useSelector(
    createMovieSelector(movieId)
  );
  const { isMovieFilesFetching, movieFilesError, hasMovieFiles } = useSelector(
    createMovieFilesSelector()
  );
  const { isExtraFilesFetching, extraFilesError } = useSelector(
    createExtraFilesSelector()
  );
  const { isMovieCreditsFetching, movieCreditsError } = useSelector(
    createMovieCreditsSelector()
  );
  const { movieRuntimeFormat } = useSelector(createUISettingsSelector());
  const isSidebarVisible = useSelector(
    (state: AppState) => state.app.isSidebarVisible
  );
  const { isSmallScreen } = useSelector(createDimensionsSelector());

  const commands = useSelector(createCommandsSelector());

  const { isRefreshing, isRenaming, isSearching } = useMemo(() => {
    const movieRefreshingCommand = findCommand(commands, {
      name: commandNames.REFRESH_MOVIE,
    });

    const isMovieRefreshingCommandExecuting = isCommandExecuting(
      movieRefreshingCommand
    );

    const allMoviesRefreshing =
      isMovieRefreshingCommandExecuting &&
      !movieRefreshingCommand?.body.movieIds?.length;

    const isMovieRefreshing =
      isMovieRefreshingCommandExecuting &&
      movieRefreshingCommand?.body.movieIds?.includes(movieId);

    const isSearchingExecuting = isCommandExecuting(
      findCommand(commands, {
        name: commandNames.MOVIE_SEARCH,
        movieIds: [movieId],
      })
    );

    const isRenamingFiles = isCommandExecuting(
      findCommand(commands, {
        name: commandNames.RENAME_FILES,
        movieId,
      })
    );

    const isRenamingMovieCommand = findCommand(commands, {
      name: commandNames.RENAME_MOVIE,
    });

    const isRenamingMovie =
      isCommandExecuting(isRenamingMovieCommand) &&
      isRenamingMovieCommand?.body?.movieIds?.includes(movieId);

    return {
      isRefreshing: isMovieRefreshing || allMoviesRefreshing,
      isRenaming: isRenamingFiles || isRenamingMovie,
      isSearching: isSearchingExecuting,
    };
  }, [movieId, commands]);

  const touchStart = useRef<number | null>(null);
  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isManageMoviesModalOpen, setIsManageMoviesModalOpen] = useState(false);
  const [isInteractiveSearchModalOpen, setIsInteractiveSearchModalOpen] =
    useState(false);
  const [isEditMovieModalOpen, setIsEditMovieModalOpen] = useState(false);
  const [isDeleteMovieModalOpen, setIsDeleteMovieModalOpen] = useState(false);
  const [isMovieHistoryModalOpen, setIsMovieHistoryModalOpen] = useState(false);
  const [titleRef, { width: titleWidth }] = useMeasure();
  const [overviewRef, { height: overviewHeight }] = useMeasure();
  const wasRefreshing = usePrevious(isRefreshing);
  const wasRenaming = usePrevious(isRenaming);

  const handleOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, []);

  const handleOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, []);

  const handleManageMoviesPress = useCallback(() => {
    setIsManageMoviesModalOpen(true);
  }, []);

  const handleManageMoviesModalClose = useCallback(() => {
    setIsManageMoviesModalOpen(false);
  }, []);

  const handleInteractiveSearchPress = useCallback(() => {
    setIsInteractiveSearchModalOpen(true);
  }, []);

  const handleInteractiveSearchModalClose = useCallback(() => {
    setIsInteractiveSearchModalOpen(false);
  }, []);

  const handleEditMoviePress = useCallback(() => {
    setIsEditMovieModalOpen(true);
  }, []);

  const handleEditMovieModalClose = useCallback(() => {
    setIsEditMovieModalOpen(false);
  }, []);

  const handleDeleteMoviePress = useCallback(() => {
    setIsEditMovieModalOpen(false);
    setIsDeleteMovieModalOpen(true);
  }, []);

  const handleDeleteMovieModalClose = useCallback(() => {
    setIsDeleteMovieModalOpen(false);
  }, []);

  const handleMovieHistoryPress = useCallback(() => {
    setIsMovieHistoryModalOpen(true);
  }, []);

  const handleMovieHistoryModalClose = useCallback(() => {
    setIsMovieHistoryModalOpen(false);
  }, []);

  const handleMonitorTogglePress = useCallback(
    (value: boolean) => {
      dispatch(
        toggleMovieMonitored({
          movieId,
          monitored: value,
        })
      );
    },
    [movieId, dispatch]
  );

  const handleRefreshPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.REFRESH_MOVIE,
        movieIds: [movieId],
      })
    );
  }, [movieId, dispatch]);

  const handleSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.MOVIE_SEARCH,
        movieIds: [movieId],
      })
    );
  }, [movieId, dispatch]);

  const handleTouchStart = useCallback(
    (event: TouchEvent) => {
      const touches = event.touches;
      const currentTouch = touches[0].pageX;
      const touchY = touches[0].pageY;

      // Only change when swipe is on header, we need horizontal scroll on tables
      if (touchY > 470) {
        return;
      }

      if (touches.length !== 1) {
        return;
      }

      if (
        currentTouch < 50 ||
        isSidebarVisible ||
        isOrganizeModalOpen ||
        isEditMovieModalOpen ||
        isDeleteMovieModalOpen ||
        isManageMoviesModalOpen ||
        isInteractiveSearchModalOpen ||
        isMovieHistoryModalOpen
      ) {
        return;
      }

      touchStart.current = currentTouch;
    },
    [
      isSidebarVisible,
      isOrganizeModalOpen,
      isEditMovieModalOpen,
      isDeleteMovieModalOpen,
      isManageMoviesModalOpen,
      isInteractiveSearchModalOpen,
      isMovieHistoryModalOpen,
    ]
  );

  const handleTouchEnd = useCallback(
    (event: TouchEvent) => {
      const touches = event.changedTouches;
      const currentTouch = touches[0].pageX;

      if (!touchStart.current) {
        return;
      }

      if (
        currentTouch > touchStart.current &&
        currentTouch - touchStart.current > 100 &&
        previousMovie !== undefined
      ) {
        history.push(getPathWithUrlBase(`/movie/${previousMovie.titleSlug}`));
      } else if (
        currentTouch < touchStart.current &&
        touchStart.current - currentTouch > 100 &&
        nextMovie !== undefined
      ) {
        history.push(getPathWithUrlBase(`/movie/${nextMovie.titleSlug}`));
      }

      touchStart.current = null;
    },
    [previousMovie, nextMovie, history]
  );

  const handleTouchCancel = useCallback(() => {
    touchStart.current = null;
  }, []);

  const handleTouchMove = useCallback(() => {
    if (!touchStart.current) {
      return;
    }
  }, []);

  const handleKeyUp = useCallback(
    (event: KeyboardEvent) => {
      if (
        isOrganizeModalOpen ||
        isManageMoviesModalOpen ||
        isInteractiveSearchModalOpen ||
        isEditMovieModalOpen ||
        isDeleteMovieModalOpen ||
        isMovieHistoryModalOpen
      ) {
        return;
      }

      if (event.composedPath && event.composedPath().length === 4) {
        if (event.key === 'ArrowLeft' && previousMovie !== undefined) {
          history.push(getPathWithUrlBase(`/movie/${previousMovie.titleSlug}`));
        }

        if (event.key === 'ArrowRight' && nextMovie !== undefined) {
          history.push(getPathWithUrlBase(`/movie/${nextMovie.titleSlug}`));
        }
      }
    },
    [
      isOrganizeModalOpen,
      isManageMoviesModalOpen,
      isInteractiveSearchModalOpen,
      isEditMovieModalOpen,
      isDeleteMovieModalOpen,
      isMovieHistoryModalOpen,
      previousMovie,
      nextMovie,
      history,
    ]
  );

  const populate = useCallback(() => {
    dispatch(fetchMovieFiles({ movieId }));
    dispatch(fetchExtraFiles({ movieId }));
    dispatch(fetchMovieCredits({ movieId }));
    dispatch(fetchQueueDetails({ movieId }));
    dispatch(fetchImportListSchema());
    dispatch(fetchRootFolders());
  }, [movieId, dispatch]);

  useEffect(() => {
    populate();
  }, [populate]);

  useEffect(() => {
    registerPagePopulator(populate, ['movieUpdated']);

    return () => {
      unregisterPagePopulator(populate);
      dispatch(clearMovieFiles());
      dispatch(clearExtraFiles());
      dispatch(clearMovieCredits());
      dispatch(clearQueueDetails());
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
    };
  }, [populate, dispatch]);

  useEffect(() => {
    if ((!isRefreshing && wasRefreshing) || (!isRenaming && wasRenaming)) {
      populate();
    }
  }, [isRefreshing, wasRefreshing, isRenaming, wasRenaming, populate]);

  useEffect(() => {
    window.addEventListener('touchstart', handleTouchStart);
    window.addEventListener('touchend', handleTouchEnd);
    window.addEventListener('touchcancel', handleTouchCancel);
    window.addEventListener('touchmove', handleTouchMove);
    window.addEventListener('keyup', handleKeyUp);

    return () => {
      window.removeEventListener('touchstart', handleTouchStart);
      window.removeEventListener('touchend', handleTouchEnd);
      window.removeEventListener('touchcancel', handleTouchCancel);
      window.removeEventListener('touchmove', handleTouchMove);
      window.removeEventListener('keyup', handleKeyUp);
    };
  }, [
    handleTouchStart,
    handleTouchEnd,
    handleTouchCancel,
    handleTouchMove,
    handleKeyUp,
  ]);

  if (!movie) {
    return null;
  }

  const {
    id,
    tmdbId,
    imdbId,
    title,
    originalTitle,
    year,
    inCinemas,
    physicalRelease,
    digitalRelease,
    runtime,
    certification,
    ratings,
    path,
    statistics = {} as Statistics,
    qualityProfileId,
    monitored,
    studio,
    originalLanguage,
    genres = [],
    collection,
    overview,
    status,
    youTubeTrailerId,
    isAvailable,
    images,
    tags,
    isSaving = false,
  } = movie;

  const { sizeOnDisk = 0 } = statistics;

  const statusDetails = getMovieStatusDetails(status);

  const fanartUrl = getFanartUrl(images);
  const isFetching =
    isMovieFilesFetching || isExtraFilesFetching || isMovieCreditsFetching;

  const marqueeWidth = isSmallScreen ? titleWidth : titleWidth - 150;

  const titleWithYear = `${title}${year > 0 ? ` (${year})` : ''}`;

  return (
    <PageContent title={titleWithYear}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('RefreshAndScan')}
            iconName={icons.REFRESH}
            spinningName={icons.REFRESH}
            title={translate('RefreshInformationAndScanDisk')}
            isSpinning={isRefreshing}
            onPress={handleRefreshPress}
          />

          <PageToolbarButton
            label={translate('SearchMovie')}
            iconName={icons.SEARCH}
            isSpinning={isSearching}
            title={undefined}
            onPress={handleSearchPress}
          />

          <PageToolbarButton
            label={translate('InteractiveSearch')}
            iconName={icons.INTERACTIVE}
            isSpinning={isSearching}
            title={undefined}
            onPress={handleInteractiveSearchPress}
          />

          <PageToolbarSeparator />

          <PageToolbarButton
            label={translate('PreviewRename')}
            iconName={icons.ORGANIZE}
            isDisabled={!hasMovieFiles}
            onPress={handleOrganizePress}
          />

          <PageToolbarButton
            label={translate('ManageFiles')}
            iconName={icons.MOVIE_FILE}
            onPress={handleManageMoviesPress}
          />

          <PageToolbarButton
            label={translate('History')}
            iconName={icons.HISTORY}
            onPress={handleMovieHistoryPress}
          />

          <PageToolbarSeparator />

          <PageToolbarButton
            label={translate('Edit')}
            iconName={icons.EDIT}
            onPress={handleEditMoviePress}
          />

          <PageToolbarButton
            label={translate('Delete')}
            iconName={icons.DELETE}
            onPress={handleDeleteMoviePress}
          />
        </PageToolbarSection>
      </PageToolbar>

      <PageContentBody innerClassName={styles.innerContentBody}>
        <div className={styles.header}>
          <div
            className={styles.backdrop}
            style={
              fanartUrl ? { backgroundImage: `url(${fanartUrl})` } : undefined
            }
          >
            <div className={styles.backdropOverlay} />
          </div>

          <div className={styles.headerContent}>
            <MoviePoster
              className={styles.poster}
              images={images}
              size={500}
              lazy={false}
            />

            <div className={styles.info}>
              <div ref={titleRef} className={styles.titleRow}>
                <div className={styles.titleContainer}>
                  <div className={styles.toggleMonitoredContainer}>
                    <MonitorToggleButton
                      className={styles.monitorToggleButton}
                      monitored={monitored}
                      isSaving={isSaving}
                      size={40}
                      onPress={handleMonitorTogglePress}
                    />
                  </div>

                  <div className={styles.title} style={{ width: marqueeWidth }}>
                    <Marquee text={title} title={originalTitle} />
                  </div>
                </div>

                <div className={styles.movieNavigationButtons}>
                  <IconButton
                    className={styles.movieNavigationButton}
                    name={icons.ARROW_LEFT}
                    size={30}
                    title={translate('MovieDetailsGoTo', {
                      title: previousMovie.title,
                    })}
                    to={`/movie/${previousMovie.titleSlug}`}
                  />

                  <IconButton
                    className={styles.movieNavigationButton}
                    name={icons.ARROW_RIGHT}
                    size={30}
                    title={translate('MovieDetailsGoTo', {
                      title: nextMovie.title,
                    })}
                    to={`/movie/${nextMovie.titleSlug}`}
                  />
                </div>
              </div>

              <div className={styles.details}>
                <div>
                  {certification ? (
                    <span
                      className={styles.certification}
                      title={translate('Certification')}
                    >
                      {certification}
                    </span>
                  ) : null}

                  <span className={styles.year}>
                    <Popover
                      anchor={
                        year > 0 ? (
                          year
                        ) : (
                          <Icon
                            name={icons.WARNING}
                            kind={kinds.WARNING}
                            size={20}
                          />
                        )
                      }
                      title={translate('ReleaseDates')}
                      body={
                        <MovieReleaseDates
                          tmdbId={tmdbId}
                          inCinemas={inCinemas}
                          digitalRelease={digitalRelease}
                          physicalRelease={physicalRelease}
                        />
                      }
                      position={tooltipPositions.BOTTOM}
                    />
                  </span>

                  {runtime ? (
                    <span
                      className={styles.runtime}
                      title={translate('Runtime')}
                    >
                      {formatRuntime(runtime, movieRuntimeFormat)}
                    </span>
                  ) : null}

                  <span className={styles.links}>
                    <Tooltip
                      anchor={<Icon name={icons.EXTERNAL_LINK} size={20} />}
                      tooltip={
                        <MovieDetailsLinks
                          tmdbId={tmdbId}
                          imdbId={imdbId}
                          youTubeTrailerId={youTubeTrailerId}
                        />
                      }
                      position={tooltipPositions.BOTTOM}
                    />
                  </span>

                  {!!tags.length && (
                    <span>
                      <Tooltip
                        anchor={<Icon name={icons.TAGS} size={20} />}
                        tooltip={<MovieTags movieId={id} />}
                        position={tooltipPositions.BOTTOM}
                      />
                    </span>
                  )}
                </div>
              </div>

              <div className={styles.details}>
                {ratings.tmdb ? (
                  <span className={styles.rating}>
                    <TmdbRating ratings={ratings} iconSize={20} />
                  </span>
                ) : null}
                {ratings.imdb ? (
                  <span className={styles.rating}>
                    <ImdbRating ratings={ratings} iconSize={20} />
                  </span>
                ) : null}
                {ratings.rottenTomatoes ? (
                  <span className={styles.rating}>
                    <RottenTomatoRating ratings={ratings} iconSize={20} />
                  </span>
                ) : null}
                {ratings.trakt ? (
                  <span className={styles.rating}>
                    <TraktRating ratings={ratings} iconSize={20} />
                  </span>
                ) : null}
              </div>

              <div>
                <InfoLabel
                  className={styles.detailsInfoLabel}
                  name={translate('Path')}
                  size={sizes.LARGE}
                >
                  <span className={styles.path}>{path}</span>
                </InfoLabel>

                <InfoLabel
                  className={styles.detailsInfoLabel}
                  name={translate('Status')}
                  title={statusDetails.message}
                  size={sizes.LARGE}
                >
                  <span className={styles.statusName}>
                    <MovieStatusLabel
                      movieId={id}
                      monitored={monitored}
                      isAvailable={isAvailable}
                      hasMovieFiles={hasMovieFiles}
                      status={status}
                    />
                  </span>
                </InfoLabel>

                <InfoLabel
                  className={styles.detailsInfoLabel}
                  name={translate('QualityProfile')}
                  size={sizes.LARGE}
                >
                  <span className={styles.qualityProfileName}>
                    <QualityProfileName qualityProfileId={qualityProfileId} />
                  </span>
                </InfoLabel>

                <InfoLabel
                  className={styles.detailsInfoLabel}
                  name={translate('Size')}
                  size={sizes.LARGE}
                >
                  <span className={styles.sizeOnDisk}>
                    {formatBytes(sizeOnDisk)}
                  </span>
                </InfoLabel>

                {collection ? (
                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    name={translate('Collection')}
                    size={sizes.LARGE}
                  >
                    <div className={styles.collection}>
                      <MovieCollectionLabel tmdbId={collection.tmdbId} />
                    </div>
                  </InfoLabel>
                ) : null}

                {originalLanguage?.name && !isSmallScreen ? (
                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    name={translate('OriginalLanguage')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.originalLanguage}>
                      {originalLanguage.name}
                    </span>
                  </InfoLabel>
                ) : null}

                {studio && !isSmallScreen ? (
                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    name={translate('Studio')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.studio}>{studio}</span>
                  </InfoLabel>
                ) : null}

                {genres.length && !isSmallScreen ? (
                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    name={translate('Genres')}
                    size={sizes.LARGE}
                  >
                    <MovieGenres className={styles.genres} genres={genres} />
                  </InfoLabel>
                ) : null}
              </div>

              <div ref={overviewRef} className={styles.overview}>
                <TextTruncate
                  line={Math.floor(
                    overviewHeight / (defaultFontSize * lineHeight)
                  )}
                  text={overview}
                />
              </div>
            </div>
          </div>
        </div>

        <div className={styles.contentContainer}>
          {!isFetching && movieFilesError ? (
            <Alert kind={kinds.DANGER}>
              {translate('LoadingMovieFilesFailed')}
            </Alert>
          ) : null}

          {!isFetching && extraFilesError ? (
            <Alert kind={kinds.DANGER}>
              {translate('LoadingMovieExtraFilesFailed')}
            </Alert>
          ) : null}

          {!isFetching && movieCreditsError ? (
            <Alert kind={kinds.DANGER}>
              {translate('LoadingMovieCreditsFailed')}
            </Alert>
          ) : null}

          <FieldSet legend={translate('Files')}>
            <MovieFileEditorTable movieId={id} />

            <ExtraFileTable movieId={id} />
          </FieldSet>

          <FieldSet legend={translate('Cast')}>
            <MovieCastPosters isSmallScreen={isSmallScreen} />
          </FieldSet>

          <FieldSet legend={translate('Crew')}>
            <MovieCrewPosters isSmallScreen={isSmallScreen} />
          </FieldSet>

          <FieldSet legend={translate('Titles')}>
            <MovieTitlesTable movieId={id} />
          </FieldSet>
        </div>

        <OrganizePreviewModal
          isOpen={isOrganizeModalOpen}
          movieId={id}
          onModalClose={handleOrganizeModalClose}
        />

        <EditMovieModal
          isOpen={isEditMovieModalOpen}
          movieId={id}
          onModalClose={handleEditMovieModalClose}
          onDeleteMoviePress={handleDeleteMoviePress}
        />

        <MovieHistoryModal
          isOpen={isMovieHistoryModalOpen}
          movieId={id}
          onModalClose={handleMovieHistoryModalClose}
        />

        <DeleteMovieModal
          isOpen={isDeleteMovieModalOpen}
          movieId={id}
          onModalClose={handleDeleteMovieModalClose}
        />

        <InteractiveImportModal
          isOpen={isManageMoviesModalOpen}
          movieId={id}
          title={title}
          folder={path}
          initialSortKey="relativePath"
          initialSortDirection={sortDirections.ASCENDING}
          showMovie={false}
          allowMovieChange={false}
          showDelete={true}
          showImportMode={false}
          modalTitle={translate('ManageFiles')}
          onModalClose={handleManageMoviesModalClose}
        />

        <MovieInteractiveSearchModal
          isOpen={isInteractiveSearchModalOpen}
          movieId={id}
          movieTitle={title}
          onModalClose={handleInteractiveSearchModalClose}
        />
      </PageContentBody>
    </PageContent>
  );
}

export default MovieDetails;
