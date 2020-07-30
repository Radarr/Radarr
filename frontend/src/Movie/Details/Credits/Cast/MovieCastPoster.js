import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import EditNetImportModalConnector from 'Settings/NetImport/NetImport/EditNetImportModalConnector';
import styles from '../MovieCreditPoster.css';

class MovieCastPoster extends Component {

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
      character,
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

    const contentStyle = {
      width: `${posterWidth}px`
    };

    return (
      <div
        className={styles.content}
        style={contentStyle}
      >
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
          {character}
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

MovieCastPoster.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  personName: PropTypes.string.isRequired,
  character: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  netImportId: PropTypes.number.isRequired,
  onNetImportSelect: PropTypes.func.isRequired
};

MovieCastPoster.defaultProps = {
  netImportId: 0
};

export default MovieCastPoster;
