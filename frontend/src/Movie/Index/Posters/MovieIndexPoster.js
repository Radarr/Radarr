import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import MoviePoster from 'Movie/MoviePoster';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import MovieIndexProgressBar from 'Movie/Index/ProgressBar/MovieIndexProgressBar';
import MovieIndexPosterInfo from './MovieIndexPosterInfo';
import styles from './MovieIndexPoster.css';

class MovieIndexPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: false
    };
  }

  //
  // Listeners

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
      id,
      title,
      monitored,
      hasFile,
      status,
      titleSlug,
      images,
      posterWidth,
      posterHeight,
      detailedProgressBar,
      showTitle,
      showMonitored,
      showQualityProfile,
      qualityProfile,
      showSearchAction,
      showRelativeDates,
      shortDateFormat,
      timeFormat,
      isRefreshingMovie,
      isSearchingMovie,
      onRefreshMoviePress,
      onSearchPress,
      ...otherProps
    } = this.props;

    const {
      hasPosterError,
      isEditMovieModalOpen,
      isDeleteMovieModalOpen
    } = this.state;

    const link = `/movie/${titleSlug}`;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    return (
      <div className={styles.container} style={style}>
        <div className={styles.content}>
          <div className={styles.posterContainer}>
            <Label className={styles.controls}>
              <SpinnerIconButton
                className={styles.action}
                name={icons.REFRESH}
                title="Refresh movie"
                isSpinning={isRefreshingMovie}
                onPress={onRefreshMoviePress}
              />

              {
                showSearchAction &&
                  <SpinnerIconButton
                    className={styles.action}
                    name={icons.SEARCH}
                    title="Search for movie"
                    isSpinning={isSearchingMovie}
                    onPress={onSearchPress}
                  />
              }

              <IconButton
                className={styles.action}
                name={icons.EDIT}
                title="Edit movie"
                onPress={this.onEditMoviePress}
              />
            </Label>

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
              to={link}
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

          <MovieIndexProgressBar
            monitored={monitored}
            hasFile={hasFile}
            status={status}
            posterWidth={posterWidth}
            detailedProgressBar={detailedProgressBar}
          />

          {
            showTitle &&
              <div className={styles.title}>
                {title}
              </div>
          }

          {
            showMonitored &&
              <div className={styles.title}>
                {monitored ? 'Monitored' : 'Unmonitored'}
              </div>
          }

          {
            showQualityProfile &&
              <div className={styles.title}>
                {qualityProfile.name}
              </div>
          }

          <MovieIndexPosterInfo
            qualityProfile={qualityProfile}
            showQualityProfile={showQualityProfile}
            showRelativeDates={showRelativeDates}
            shortDateFormat={shortDateFormat}
            timeFormat={timeFormat}
            {...otherProps}
          />

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
        </div>
      </div>
    );
  }
}

MovieIndexPoster.propTypes = {
  style: PropTypes.object.isRequired,
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isRefreshingMovie: PropTypes.bool.isRequired,
  isSearchingMovie: PropTypes.bool.isRequired,
  onRefreshMoviePress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

MovieIndexPoster.defaultProps = {
  statistics: {
    seasonCount: 0,
    episodeCount: 0,
    episodeFileCount: 0,
    totalEpisodeCount: 0
  }
};

export default MovieIndexPoster;
