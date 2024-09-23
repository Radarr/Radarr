import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import ImdbRating from 'Components/ImdbRating';
import ImportListListConnector from 'Components/ImportListListConnector';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import TmdbRating from 'Components/TmdbRating';
import Popover from 'Components/Tooltip/Popover';
import TraktRating from 'Components/TraktRating';
import AddNewDiscoverMovieModal from 'DiscoverMovie/AddNewDiscoverMovieModal';
import ExcludeMovieModal from 'DiscoverMovie/Exclusion/ExcludeMovieModal';
import { icons } from 'Helpers/Props';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import MoviePopularityIndex from 'Movie/MoviePopularityIndex';
import formatRuntime from 'Utilities/Date/formatRuntime';
import translate from 'Utilities/String/translate';
import ListMovieStatusCell from './ListMovieStatusCell';
import styles from './DiscoverMovieRow.css';

class DiscoverMovieRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddMovieModalOpen: false,
      isExcludeMovieModalOpen: false
    };
  }

  //
  // Listeners

  onAddMoviePress = () => {
    this.setState({ isNewAddMovieModalOpen: true });
  };

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  };

  onExcludeMoviePress = () => {
    this.setState({ isExcludeMovieModalOpen: true });
  };

  onExcludeMovieModalClose = () => {
    this.setState({ isExcludeMovieModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      status,
      tmdbId,
      imdbId,
      youTubeTrailerId,
      title,
      originalLanguage,
      studio,
      inCinemas,
      physicalRelease,
      digitalRelease,
      runtime,
      year,
      overview,
      folder,
      images,
      genres,
      ratings,
      popularity,
      certification,
      movieRuntimeFormat,
      collection,
      columns,
      isExisting,
      isExcluded,
      isRecommendation,
      isTrending,
      isPopular,
      isSelected,
      lists,
      onSelectedChange
    } = this.props;

    const {
      isNewAddMovieModalOpen,
      isExcludeMovieModalOpen
    } = this.state;

    const linkProps = isExisting ? { to: `/movie/${tmdbId}` } : { onPress: this.onAddMoviePress };

    return (
      <>
        <VirtualTableSelectCell
          inputClassName={styles.checkInput}
          id={tmdbId}
          key={name}
          isSelected={isSelected}
          isDisabled={false}
          onSelectedChange={onSelectedChange}
        />

        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'status') {
              return (
                <ListMovieStatusCell
                  key={name}
                  className={styles[name]}
                  status={status}
                  isExclusion={isExcluded}
                  isExisting={isExisting}
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
                  <Link
                    {...linkProps}
                  >
                    {title}
                  </Link>

                  {
                    isExisting ?
                      <Icon
                        className={styles.alreadyExistsIcon}
                        name={icons.CHECK_CIRCLE}
                        title={translate('AlreadyInYourLibrary')}
                      /> : null
                  }

                  {
                    isExcluded ?
                      <Icon
                        className={styles.exclusionIcon}
                        name={icons.DANGER}
                        title={translate('MovieExcludedFromAutomaticAdd')}
                      /> : null
                  }
                </VirtualTableRowCell>
              );
            }

            if (name === 'collection') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {collection ? collection.title : null }
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

            if (name === 'inCinemas') {
              return (
                <RelativeDateCell
                  key={name}
                  className={styles[name]}
                  date={inCinemas}
                  timeForToday={false}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'physicalRelease') {
              return (
                <RelativeDateCell
                  key={name}
                  className={styles[name]}
                  date={physicalRelease}
                  timeForToday={false}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'digitalRelease') {
              return (
                <RelativeDateCell
                  key={name}
                  className={styles[name]}
                  date={digitalRelease}
                  timeForToday={false}
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

            if (name === 'tmdbRating') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {ratings.tmdb ? <TmdbRating ratings={ratings} /> : null}
                </VirtualTableRowCell>
              );
            }

            if (name === 'imdbRating') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {ratings.imdb ? <ImdbRating ratings={ratings} /> : null}
                </VirtualTableRowCell>
              );
            }

            if (name === 'rottenTomatoesRating') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {ratings.rottenTomatoes ? <RottenTomatoRating ratings={ratings} /> : null}
                </VirtualTableRowCell>
              );
            }

            if (name === 'traktRating') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {ratings.trakt ? <TraktRating ratings={ratings} /> : null}
                </VirtualTableRowCell>
              );
            }

            if (name === 'popularity') {
              return (
                <VirtualTableRowCell key={name} className={styles[name]}>
                  <MoviePopularityIndex popularity={popularity} />
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

            if (name === 'lists') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <ImportListListConnector
                    lists={lists}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'isRecommendation') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {
                    isRecommendation ?
                      <Icon
                        className={styles.statusIcon}
                        name={icons.RECOMMENDED}
                        size={12}
                        title={translate('MovieIsRecommend')}
                      /> :
                      null
                  }
                </VirtualTableRowCell>
              );
            }

            if (name === 'isTrending') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {
                    isTrending ?
                      <Icon
                        className={styles.statusIcon}
                        name={icons.TRENDING}
                        size={12}
                        title={translate('MovieIsTrending')}
                      /> :
                      null
                  }
                </VirtualTableRowCell>
              );
            }

            if (name === 'isPopular') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {
                    isPopular ?
                      <Icon
                        className={styles.statusIcon}
                        name={icons.POPULAR}
                        size={12}
                        title={translate('MovieIsPopular')}
                      /> :
                      null
                  }
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
                    <Popover
                      anchor={
                        <Icon
                          name={icons.EXTERNAL_LINK}
                          size={12}
                        />
                      }
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

                  <IconButton
                    name={icons.REMOVE}
                    title={isExcluded ? translate('MovieAlreadyExcluded') : translate('ExcludeMovie')}
                    onPress={this.onExcludeMoviePress}
                    isDisabled={isExcluded}
                  />
                </VirtualTableRowCell>
              );
            }

            return null;
          })
        }

        <AddNewDiscoverMovieModal
          isOpen={isNewAddMovieModalOpen && !isExisting}
          tmdbId={tmdbId}
          title={title}
          year={year}
          overview={overview}
          folder={folder}
          images={images}
          onModalClose={this.onAddMovieModalClose}
        />

        <ExcludeMovieModal
          isOpen={isExcludeMovieModalOpen}
          tmdbId={tmdbId}
          title={title}
          year={year}
          onModalClose={this.onExcludeMovieModalClose}
        />
      </>
    );
  }
}

DiscoverMovieRow.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  status: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  originalLanguage: PropTypes.object.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  folder: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  studio: PropTypes.string,
  inCinemas: PropTypes.string,
  physicalRelease: PropTypes.string,
  digitalRelease: PropTypes.string,
  runtime: PropTypes.number,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  ratings: PropTypes.object.isRequired,
  popularity: PropTypes.number.isRequired,
  certification: PropTypes.string,
  collection: PropTypes.object,
  movieRuntimeFormat: PropTypes.string.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExisting: PropTypes.bool.isRequired,
  isExcluded: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  isRecommendation: PropTypes.bool.isRequired,
  isPopular: PropTypes.bool.isRequired,
  isTrending: PropTypes.bool.isRequired,
  lists: PropTypes.arrayOf(PropTypes.number).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

DiscoverMovieRow.defaultProps = {
  genres: [],
  lists: []
};

export default DiscoverMovieRow;
