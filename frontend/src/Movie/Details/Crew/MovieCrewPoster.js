import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import Label from 'Components/Label';
import MovieHeadshot from 'Movie/MovieHeadshot';
import styles from './MovieCrewPoster.css';

class MovieCrewPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditMovieModalOpen: false
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
      crewName,
      job,
      images,
      posterWidth,
      posterHeight
    } = this.props;

    const {
      hasPosterError
    } = this.state;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    return (
      <div className={styles.content}>
        <div className={styles.posterContainer}>
          <Label className={styles.controls}>
            <IconButton
              className={styles.action}
              name={icons.EDIT}
              title="Edit movie"
              onPress={this.onEditMoviePress}
            />
          </Label>

          <div
            className={styles.poster}
            style={elementStyle}
          >
            <MovieHeadshot
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
                  {crewName}
                </div>
            }
          </div>
        </div>

        <div className={styles.title}>
          {crewName}
        </div>
        <div className={styles.title}>
          {job}
        </div>
      </div>
    );
  }
}

MovieCrewPoster.propTypes = {
  crewId: PropTypes.number.isRequired,
  crewName: PropTypes.string.isRequired,
  job: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired
};

export default MovieCrewPoster;
