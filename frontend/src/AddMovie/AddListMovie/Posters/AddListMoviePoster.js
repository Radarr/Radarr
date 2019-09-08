import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import MoviePoster from 'Movie/MoviePoster';
import AddNewMovieModal from 'AddMovie/AddNewMovie/AddNewMovieModal';
import styles from './AddListMoviePoster.css';

class AddListMoviePoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isNewAddMovieModalOpen: false
    };
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddMovieModalOpen: true });
  }

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  }

  onPosterLoad = () => {
    if (this.state.hasPosterError) {
      this.setState({ hasPosterError: false });
    }
  }

  onPosterLoadError = () => {
    if (!this.state.hasPosterError) {
      this.setState({ hasPosterError: true });
    }
  }

  //
  // Render

  render() {
    const {
      style,
      tmdbId,
      title,
      year,
      overview,
      folder,
      status,
      titleSlug,
      images,
      posterWidth,
      posterHeight,
      showTitle,
      isExistingMovie
    } = this.props;

    const {
      hasPosterError,
      isNewAddMovieModalOpen
    } = this.state;

    const linkProps = isExistingMovie ? { to: `/movie/${titleSlug}` } : { onPress: this.onPress };

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    return (
      <div className={styles.container} style={style}>
        <div className={styles.content}>
          <div className={styles.posterContainer}>
            {
              status === 'ended' &&
                <div
                  className={styles.ended}
                  title="Ended"
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

          {
            showTitle &&
              <div className={styles.title}>
                {title}
              </div>
          }

          <AddNewMovieModal
            isOpen={isNewAddMovieModalOpen && !isExistingMovie}
            tmdbId={tmdbId}
            title={title}
            year={year}
            overview={overview}
            folder={folder}
            images={images}
            onModalClose={this.onAddMovieModalClose}
          />
        </div>
      </div>
    );
  }
}

AddListMoviePoster.propTypes = {
  style: PropTypes.object.isRequired,
  tmdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  folder: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isExistingMovie: PropTypes.bool.isRequired,
  isExclusionMovie: PropTypes.bool.isRequired
};

AddListMoviePoster.defaultProps = {
  statistics: {
    movieFileCount: 0
  }
};

export default AddListMoviePoster;
