import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import Label from 'Components/Label';
import MovieHeadshot from 'Movie/MovieHeadshot';
import EditNetImportModalConnector from 'Settings/NetImport/NetImport/EditNetImportModalConnector';
import styles from '../MovieCreditPoster.css';

class MovieCrewPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditNetImportModalOpen: false
    };
  }

  //
  // Listeners

  onEditNetImportPress = () => {
    this.setState({ isEditNetImportModalOpen: true });
  }

  onAddNetImportPress = () => {
    this.props.onNetImportSelect();
    this.setState({ isEditNetImportModalOpen: true });
  }

  onEditNetImportModalClose = () => {
    this.setState({ isEditNetImportModalOpen: false });
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
      personName,
      job,
      images,
      posterWidth,
      posterHeight,
      netImportId
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
            {
              netImportId > 0 ?
                <IconButton
                  className={styles.action}
                  name={icons.EDIT}
                  title="Edit Person"
                  onPress={this.onEditNetImportPress}
                /> :
                <IconButton
                  className={styles.action}
                  name={icons.ADD}
                  title="Follow Person"
                  onPress={this.onAddNetImportPress}
                />
            }
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
                  {personName}
                </div>
            }
          </div>
        </div>

        <div className={styles.title}>
          {personName}
        </div>
        <div className={styles.title}>
          {job}
        </div>

        <EditNetImportModalConnector
          id={netImportId}
          isOpen={this.state.isEditNetImportModalOpen}
          onModalClose={this.onEditNetImportModalClose}
          onDeleteNetImportPress={this.onDeleteNetImportPress}
        />
      </div>
    );
  }
}

MovieCrewPoster.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  personName: PropTypes.string.isRequired,
  job: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  netImportId: PropTypes.number.isRequired,
  onNetImportSelect: PropTypes.func.isRequired
};

MovieCrewPoster.defaultProps = {
  netImportId: 0
};

export default MovieCrewPoster;
