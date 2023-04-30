import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { SelectActionType, useSelect } from 'App/SelectContext';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Icon from 'Components/Icon';
import ImdbRating from 'Components/ImdbRating';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import Column from 'Components/Table/Column';
import TagListConnector from 'Components/TagListConnector';
import TmdbRating from 'Components/TmdbRating';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds } from 'Helpers/Props';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import createMovieIndexItemSelector from 'Movie/Index/createMovieIndexItemSelector';
import MovieFileStatusConnector from 'Movie/MovieFileStatusConnector';
import MovieTitleLink from 'Movie/MovieTitleLink';
import { executeCommand } from 'Store/Actions/commandActions';
import formatRuntime from 'Utilities/Date/formatRuntime';
import formatBytes from 'Utilities/Number/formatBytes';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import MovieStatusCell from './MovieStatusCell';
import selectTableOptions from './selectTableOptions';
import styles from './MovieIndexRow.css';

interface MovieIndexRowProps {
  movieId: number;
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

function MovieIndexRow(props: MovieIndexRowProps) {
  const { movieId, columns, isSelectMode } = props;

  const { movie, qualityProfile, isRefreshingMovie, isSearchingMovie } =
    useSelector(createMovieIndexItemSelector(props.movieId));

  const { showSearchAction } = useSelector(selectTableOptions);

  const {
    monitored,
    titleSlug,
    title,
    collection,
    studio,
    originalLanguage,
    originalTitle,
    added,
    year,
    inCinemas,
    digitalRelease,
    physicalRelease,
    runtime,
    minimumAvailability,
    path,
    sizeOnDisk,
    genres = [],
    queueStatus,
    queueState,
    ratings,
    certification,
    tags = [],
    tmdbId,
    imdbId,
    isAvailable,
    grabbed,
    movieFile,
    youTubeTrailerId,
    isSaving = false,
    movieRuntimeFormat,
  } = movie;

  const dispatch = useDispatch();
  const [isEditMovieModalOpen, setIsEditMovieModalOpen] = useState(false);
  const [isDeleteMovieModalOpen, setIsDeleteMovieModalOpen] = useState(false);
  const [selectState, selectDispatch] = useSelect();

  const onRefreshPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_MOVIE,
        movieId,
      })
    );
  }, [movieId, dispatch]);

  const onSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: MOVIE_SEARCH,
        movieId,
      })
    );
  }, [movieId, dispatch]);

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

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey }) => {
      selectDispatch({
        type: SelectActionType.ToggleSelected,
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [selectDispatch]
  );

  return (
    <>
      {isSelectMode ? (
        <VirtualTableSelectCell
          id={movieId}
          isSelected={selectState.selectedState[movieId]}
          isDisabled={false}
          onSelectedChange={onSelectedChange}
        />
      ) : null}

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'status') {
          return (
            <MovieStatusCell
              key={name}
              className={styles[name]}
              movieId={movieId}
              monitored={monitored}
              status={status}
              isSelectMode={isSelectMode}
              isSaving={isSaving}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'sortTitle') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <MovieTitleLink titleSlug={titleSlug} title={title} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'collection') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {collection ? collection.title : null}
            </VirtualTableRowCell>
          );
        }

        if (name === 'studio') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {studio}
            </VirtualTableRowCell>
          );
        }

        if (name === 'originalLanguage') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {originalLanguage.name}
            </VirtualTableRowCell>
          );
        }

        if (name === 'originalTitle') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {originalTitle}
            </VirtualTableRowCell>
          );
        }

        if (name === 'qualityProfileId') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {qualityProfile.name}
            </VirtualTableRowCell>
          );
        }

        if (name === 'added') {
          return (
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={added}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'year') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {year}
            </VirtualTableRowCell>
          );
        }

        if (name === 'inCinemas') {
          return (
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={inCinemas}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'digitalRelease') {
          return (
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={digitalRelease}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'physicalRelease') {
          return (
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={physicalRelease}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'runtime') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {formatRuntime(runtime, movieRuntimeFormat)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'minimumAvailability') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {titleCase(minimumAvailability)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'path') {
          return (
            <VirtualTableRowCell
              key={name}
              className={styles[name]}
              title={path}
            >
              {path}
            </VirtualTableRowCell>
          );
        }

        if (name === 'sizeOnDisk') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {formatBytes(sizeOnDisk)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'genres') {
          const joinedGenres = genres.join(', ');

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedGenres}>{joinedGenres}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'movieStatus') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <MovieFileStatusConnector
                isAvailable={isAvailable}
                monitored={monitored}
                grabbed={grabbed}
                movieFile={movieFile}
                queueStatus={queueStatus}
                queueState={queueState}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'tmdbRating') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <TmdbRating ratings={ratings} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'rottenTomatoesRating') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <RottenTomatoRating ratings={ratings} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'imdbRating') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <ImdbRating ratings={ratings} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'certification') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {certification}
            </VirtualTableRowCell>
          );
        }

        if (name === 'tags') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <TagListConnector tags={tags} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span className={styles.externalLinks}>
                <Tooltip
                  anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
                  tooltip={
                    <MovieDetailsLinks
                      tmdbId={tmdbId}
                      imdbId={imdbId}
                      youTubeTrailerId={youTubeTrailerId}
                    />
                  }
                  canFlip={true}
                  kind={kinds.INVERSE}
                />
              </span>

              <SpinnerIconButton
                name={icons.REFRESH}
                title={translate('RefreshMovie')}
                isSpinning={isRefreshingMovie}
                onPress={onRefreshPress}
              />

              {showSearchAction && (
                <SpinnerIconButton
                  className={styles.actions}
                  name={icons.SEARCH}
                  title={translate('SearchForMovie')}
                  isSpinning={isSearchingMovie}
                  onPress={onSearchPress}
                />
              )}

              <IconButton
                name={icons.EDIT}
                title={translate('EditMovie')}
                onPress={onEditMoviePress}
              />
            </VirtualTableRowCell>
          );
        }

        return null;
      })}

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
    </>
  );
}

export default MovieIndexRow;
