import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import MovieIndexProgressBar from 'Movie/Index/ProgressBar/MovieIndexProgressBar';
import MoviePoster from 'Movie/MoviePoster';
import AddNewCollectionMovieModal from './../AddNewCollectionMovieModal';
import styles from './CollectionMovie.css';

class CollectionMovie extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditMovieModalOpen: false,
      isNewAddMovieModalOpen: false
    };
  }

  //
  // Listeners

  onEditMoviePress = () => {
    this.setState({ isEditMovieModalOpen: true });
  };

  onEditMovieModalClose = () => {
    this.setState({ isEditMovieModalOpen: false });
  };

  onAddMoviePress = () => {
    this.setState({ isNewAddMovieModalOpen: true });
  };

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
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

  //
  // Render

  render() {
    const {
      id,
      title,
      overview,
      year,
      tmdbId,
      images,
      monitored,
      hasFile,
      folder,
      isAvailable,
      isExistingMovie,
      posterWidth,
      posterHeight,
      detailedProgressBar,
      onMonitorTogglePress,
      collectionId
    } = this.props;

    const {
      isEditMovieModalOpen,
      isNewAddMovieModalOpen
    } = this.state;

    const linkProps = id ? { to: `/movie/${tmdbId}` } : { onPress: this.onAddMoviePress };

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`,
      borderRadius: '5px'
    };

    return (
      <div className={styles.content}>
        <div className={styles.posterContainer}>
          {
            isExistingMovie &&
              <div className={styles.editorSelect}>
                <MonitorToggleButton
                  className={styles.monitorToggleButton}
                  monitored={monitored}
                  size={20}
                  onPress={onMonitorTogglePress}
                />
              </div>
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

            <div className={styles.overlay}>
              <div className={styles.overlayTitle}>
                {title}
              </div>

              {
                id &&
                  <div className={styles.overlayStatus}>
                    <MovieIndexProgressBar
                      monitored={monitored}
                      hasFile={hasFile}
                      status={status}
                      bottomRadius={true}
                      posterWidth={posterWidth}
                      detailedProgressBar={detailedProgressBar}
                      isAvailable={isAvailable}
                    />
                  </div>
              }
            </div>
          </Link>
        </div>

        <AddNewCollectionMovieModal
          isOpen={isNewAddMovieModalOpen && !isExistingMovie}
          tmdbId={tmdbId}
          title={title}
          year={year}
          overview={overview}
          images={images}
          folder={folder}
          onModalClose={this.onAddMovieModalClose}
          collectionId={collectionId}
        />

        <EditMovieModalConnector
          isOpen={isEditMovieModalOpen}
          movieId={id}
          onModalClose={this.onEditMovieModalClose}
          onDeleteMoviePress={this.onDeleteMoviePress}
        />
      </div>
    );
  }
}

CollectionMovie.propTypes = {
  id: PropTypes.number,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  monitored: PropTypes.bool,
  collectionId: PropTypes.number.isRequired,
  hasFile: PropTypes.bool,
  folder: PropTypes.string,
  isAvailable: PropTypes.bool,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  isExistingMovie: PropTypes.bool,
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  onMonitorTogglePress: PropTypes.func.isRequired
};

export default CollectionMovie;
