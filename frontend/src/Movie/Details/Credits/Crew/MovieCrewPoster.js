import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import { icons } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import EditImportListModalConnector from 'Settings/ImportLists/ImportLists/EditImportListModalConnector';
import translate from 'Utilities/String/translate';
import styles from '../MovieCreditPoster.css';

class MovieCrewPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditImportListModalOpen: false
    };
  }

  //
  // Listeners

  onEditImportListPress = () => {
    this.setState({ isEditImportListModalOpen: true });
  };

  onAddImportListPress = () => {
    this.props.onImportListSelect();
    this.setState({ isEditImportListModalOpen: true });
  };

  onEditImportListModalClose = () => {
    this.setState({ isEditImportListModalOpen: false });
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
      personName,
      job,
      images,
      posterWidth,
      posterHeight,
      importListId
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
              importListId > 0 ?
                <IconButton
                  className={styles.action}
                  name={icons.EDIT}
                  title={translate('EditPerson')}
                  onPress={this.onEditImportListPress}
                /> :
                <IconButton
                  className={styles.action}
                  name={icons.ADD}
                  title={translate('FollowPerson')}
                  onPress={this.onAddImportListPress}
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
          {job}
        </div>

        <EditImportListModalConnector
          id={importListId}
          isOpen={this.state.isEditImportListModalOpen}
          onModalClose={this.onEditImportListModalClose}
          onDeleteImportListPress={this.onDeleteImportListPress}
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
  importListId: PropTypes.number.isRequired,
  onImportListSelect: PropTypes.func.isRequired
};

MovieCrewPoster.defaultProps = {
  importListId: 0
};

export default MovieCrewPoster;
