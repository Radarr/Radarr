import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import ImdbRating from 'Components/ImdbRating';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import TmdbRating from 'Components/TmdbRating';
import Popover from 'Components/Tooltip/Popover';
import TraktRating from 'Components/TraktRating';
import AddNewDiscoverMovieModal from 'DiscoverMovie/AddNewDiscoverMovieModal';
import ExcludeMovieModal from 'DiscoverMovie/Exclusion/ExcludeMovieModal';
import { icons } from 'Helpers/Props';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import MoviePoster from 'Movie/MoviePoster';
import translate from 'Utilities/String/translate';
import DiscoverMoviePosterInfo from './DiscoverMoviePosterInfo';
import styles from './DiscoverMoviePoster.css';

class DiscoverMoviePoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isNewAddMovieModalOpen: false,
      isExcludeMovieModalOpen: false
    };
  }

  //
  // Listeners

  onPress = () => {
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

  onPosterLoad = () => {
    if (this.state.hasPosterError) {
      this.setState({ hasPosterError: false });
    }
  };

  onPosterLoadError = () => {
    if (!this.state.hasPosterError) {
      this.setState({ hasPosterError: true });
    }
  };

  onChange = ({ value, shiftKey }) => {
    const {
      tmdbId,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id: tmdbId, value, shiftKey });
  };

  //
  // Render

  render() {
    const {
      tmdbId,
      imdbId,
      youTubeTrailerId,
      title,
      year,
      overview,
      folder,
      images,
      posterWidth,
      posterHeight,
      showTitle,
      showTmdbRating,
      showImdbRating,
      showRottenTomatoesRating,
      showTraktRating,
      ratings,
      isExisting,
      isExcluded,
      isSelected,
      showRelativeDates,
      shortDateFormat,
      timeFormat,
      movieRuntimeFormat,
      ...otherProps
    } = this.props;

    const {
      hasPosterError,
      isNewAddMovieModalOpen,
      isExcludeMovieModalOpen
    } = this.state;

    const linkProps = isExisting ? { to: `/movie/${tmdbId}` } : { onPress: this.onPress };

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    return (
      <div className={styles.content}>
        <div className={styles.posterContainer} title={title}>
          {
            <div className={styles.editorSelect}>
              <CheckInput
                className={styles.checkInput}
                name={tmdbId.toString()}
                value={isSelected}
                onChange={this.onChange}
              />
            </div>
          }

          <Label className={styles.controls}>
            <IconButton
              className={styles.action}
              name={icons.REMOVE}
              title={isExcluded ? translate('MovieAlreadyExcluded') : translate('ExcludeMovie')}
              onPress={this.onExcludeMoviePress}
              isDisabled={isExcluded}
            />
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
          </Label>

          {
            isExcluded &&
              <div
                className={styles.excluded}
                title={translate('Excluded')}
              />
          }

          {
            isExisting &&
              <div
                className={styles.existing}
                title={translate('Existing')}
              />
          }

          <Link
            className={styles.link}
            style={elementStyle}
            {...linkProps}
          >
            <MoviePoster
              className={styles.poster}
              style={elementStyle}
              images={images}
              size={250}
              lazy={false}
              overflow={true}
              onError={this.onPosterLoadError}
              onLoad={this.onPosterLoad}
            />

            {
              hasPosterError &&
                <div className={styles.overlayTitle}>
                  {title}
                </div>
            }
          </Link>
        </div>

        {showTitle ?
          <div className={styles.title} title={title}>
            {title}
          </div> :
          null}

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

        {showTraktRating && !!ratings.trakt ? (
          <div className={styles.title}>
            <TraktRating ratings={ratings} iconSize={12} />
          </div>
        ) : null}

        <DiscoverMoviePosterInfo
          showRelativeDates={showRelativeDates}
          shortDateFormat={shortDateFormat}
          timeFormat={timeFormat}
          movieRuntimeFormat={movieRuntimeFormat}
          ratings={ratings}
          showTmdbRating={showTmdbRating}
          showImdbRating={showImdbRating}
          showRottenTomatoesRating={showRottenTomatoesRating}
          showTraktRating={showTraktRating}
          {...otherProps}
        />

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
      </div>
    );
  }
}

DiscoverMoviePoster.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  folder: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showTmdbRating: PropTypes.bool.isRequired,
  showImdbRating: PropTypes.bool.isRequired,
  showRottenTomatoesRating: PropTypes.bool.isRequired,
  showTraktRating: PropTypes.bool.isRequired,
  ratings: PropTypes.object.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  movieRuntimeFormat: PropTypes.string.isRequired,
  isExisting: PropTypes.bool.isRequired,
  isExcluded: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default DiscoverMoviePoster;
