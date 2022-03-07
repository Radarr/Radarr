import PropTypes from 'prop-types';
import React, { Component } from 'react';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import TagListConnector from 'Components/TagListConnector';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds } from 'Helpers/Props';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import MovieFileStatusConnector from 'Movie/MovieFileStatusConnector';
import MovieTitleLink from 'Movie/MovieTitleLink';
import formatRuntime from 'Utilities/Date/formatRuntime';
import formatBytes from 'Utilities/Number/formatBytes';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import MovieStatusCell from './MovieStatusCell';
import styles from './MovieIndexRow.css';

class MovieIndexRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: false
    };
  }

  onEditMoviePress = () => {
    this.setState({ isEditMovieModalOpen: true });
  }

  onEditMovieModalClose = () => {
    this.setState({ isEditMovieModalOpen: false });
  }

  onDeleteMoviePress = () => {
    this.setState({
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: true
    });
  }

  onDeleteMovieModalClose = () => {
    this.setState({ isDeleteMovieModalOpen: false });
  }

  onUseSceneNumberingChange = () => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
    //
  }

  //
  // Render

  render() {
    const {
      id,
      tmdbId,
      imdbId,
      youTubeTrailerId,
      monitored,
      status,
      title,
      titleSlug,
      collection,
      studio,
      qualityProfile,
      added,
      year,
      inCinemas,
      physicalRelease,
      originalLanguage,
      originalTitle,
      digitalRelease,
      runtime,
      minimumAvailability,
      path,
      sizeOnDisk,
      genres,
      ratings,
      certification,
      tags,
      showSearchAction,
      columns,
      isRefreshingMovie,
      isSearchingMovie,
      isMovieEditorActive,
      isSelected,
      onRefreshMoviePress,
      onSearchPress,
      onSelectedChange,
      queueStatus,
      queueState,
      movieRuntimeFormat
    } = this.props;

    const {
      isEditMovieModalOpen,
      isDeleteMovieModalOpen
    } = this.state;

    return (
      <>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (isMovieEditorActive && name === 'select') {
              return (
                <VirtualTableSelectCell
                  inputClassName={styles.checkInput}
                  id={id}
                  key={name}
                  isSelected={isSelected}
                  isDisabled={false}
                  onSelectedChange={onSelectedChange}
                />
              );
            }

            if (name === 'status') {
              return (
                <MovieStatusCell
                  key={name}
                  className={styles[name]}
                  monitored={monitored}
                  status={status}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'sortTitle') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >

                  <MovieTitleLink
                    titleSlug={titleSlug}
                    title={title}
                  />

                </VirtualTableRowCell>
              );
            }

            if (name === 'collection') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {collection ? collection.name : null }
                </VirtualTableRowCell>
              );
            }

            if (name === 'studio') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {studio}
                </VirtualTableRowCell>
              );
            }

            if (name === 'originalLanguage') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {originalLanguage.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'originalTitle') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {originalTitle}
                </VirtualTableRowCell>
              );
            }

            if (name === 'qualityProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
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
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
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
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {formatRuntime(runtime, movieRuntimeFormat)}
                </VirtualTableRowCell>
              );
            }

            if (name === 'minimumAvailability') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
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
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {formatBytes(sizeOnDisk)}
                </VirtualTableRowCell>
              );
            }

            if (name === 'genres') {
              const joinedGenres = genres.join(', ');

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <span title={joinedGenres}>
                    {joinedGenres}
                  </span>
                </VirtualTableRowCell>
              );
            }

            if (name === 'movieStatus') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <MovieFileStatusConnector
                    movieId={id}
                    queueStatus={queueStatus}
                    queueState={queueState}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'ratings') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <HeartRating
                    ratings={ratings}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'certification') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {certification}
                </VirtualTableRowCell>
              );
            }

            if (name === 'tags') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <TagListConnector
                    tags={tags}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <span className={styles.externalLinks}>
                    <Tooltip
                      anchor={
                        <Icon
                          name={icons.EXTERNAL_LINK}
                          size={12}
                        />
                      }
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
                    onPress={onRefreshMoviePress}
                  />

                  {
                    showSearchAction &&
                      <SpinnerIconButton
                        className={styles.action}
                        name={icons.SEARCH}
                        title={translate('SearchForMovie')}
                        isSpinning={isSearchingMovie}
                        onPress={onSearchPress}
                      />
                  }

                  <IconButton
                    name={icons.EDIT}
                    title={translate('EditMovie')}
                    onPress={this.onEditMoviePress}
                  />
                </VirtualTableRowCell>
              );
            }

            return null;
          })
        }

        <EditMovieModalConnector
          isOpen={isEditMovieModalOpen}
          movieId={id}
          onModalClose={this.onEditMovieModalClose}
          onDeleteMoviePress={this.onDeleteMoviePress}
        />

        <DeleteMovieModal
          isOpen={isDeleteMovieModalOpen}
          movieId={id}
          onModalClose={this.onDeleteMovieModalClose}
        />
      </>
    );
  }
}

MovieIndexRow.propTypes = {
  id: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  originalTitle: PropTypes.string.isRequired,
  originalLanguage: PropTypes.object.isRequired,
  studio: PropTypes.string,
  collection: PropTypes.object,
  qualityProfile: PropTypes.object.isRequired,
  added: PropTypes.string,
  year: PropTypes.number,
  inCinemas: PropTypes.string,
  physicalRelease: PropTypes.string,
  digitalRelease: PropTypes.string,
  runtime: PropTypes.number,
  minimumAvailability: PropTypes.string.isRequired,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number.isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  ratings: PropTypes.object.isRequired,
  certification: PropTypes.string,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isRefreshingMovie: PropTypes.bool.isRequired,
  isSearchingMovie: PropTypes.bool.isRequired,
  onRefreshMoviePress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired,
  isMovieEditorActive: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  queueStatus: PropTypes.string,
  queueState: PropTypes.string,
  movieRuntimeFormat: PropTypes.string.isRequired
};

MovieIndexRow.defaultProps = {
  genres: [],
  tags: []
};

export default MovieIndexRow;
