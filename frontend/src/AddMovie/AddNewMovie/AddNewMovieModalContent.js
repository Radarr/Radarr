import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import MoviePoster from 'Movie/MoviePoster';
import styles from './AddNewMovieModalContent.css';

class AddNewMovieModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      searchForMovie: false
    };
  }

  //
  // Listeners

  onSearchForMissingMovieChange = ({ value }) => {
    this.setState({ searchForMovie: value });
  }

  onQualityProfileIdChange = ({ value }) => {
    this.props.onInputChange({ name: 'qualityProfileId', value: parseInt(value) });
  }

  onAddMoviePress = () => {
    this.props.onAddMoviePress(this.state.searchForMovie);
  }

  //
  // Render

  render() {
    const {
      title,
      year,
      overview,
      images,
      isAdding,
      rootFolderPath,
      monitor,
      qualityProfileId,
      minimumAvailability,
      folder,
      tags,
      isSmallScreen,
      isWindows,
      onModalClose,
      onInputChange
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {title}

          {
            !title.contains(year) && !!year &&
              <span className={styles.year}>({year})</span>
          }
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              !isSmallScreen &&
                <div className={styles.poster}>
                  <MoviePoster
                    className={styles.poster}
                    images={images}
                    size={250}
                  />
                </div>
            }

            <div className={styles.info}>
              <div className={styles.overview}>
                {overview}
              </div>

              <Form>
                <FormGroup>
                  <FormLabel>Root Folder</FormLabel>

                  <FormInputGroup
                    type={inputTypes.ROOT_FOLDER_SELECT}
                    name="rootFolderPath"
                    valueOptions={{
                      movieFolder: folder,
                      isWindows
                    }}
                    selectedValueOptions={{
                      movieFolder: folder,
                      isWindows
                    }}
                    helpText={`'${folder}' subfolder will be created automatically`}
                    onChange={onInputChange}
                    {...rootFolderPath}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>
                    Monitor
                  </FormLabel>

                  <FormInputGroup
                    type={inputTypes.MOVIE_MONITORED_SELECT}
                    name="monitor"
                    onChange={onInputChange}
                    {...monitor}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>Minimum Availability</FormLabel>

                  <FormInputGroup
                    type={inputTypes.AVAILABILITY_SELECT}
                    name="minimumAvailability"
                    onChange={onInputChange}
                    {...minimumAvailability}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>Quality Profile</FormLabel>

                  <FormInputGroup
                    type={inputTypes.QUALITY_PROFILE_SELECT}
                    name="qualityProfileId"
                    onChange={this.onQualityProfileIdChange}
                    {...qualityProfileId}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>Tags</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TAG}
                    name="tags"
                    onChange={onInputChange}
                    {...tags}
                  />
                </FormGroup>
              </Form>
            </div>
          </div>
        </ModalBody>

        <ModalFooter className={styles.modalFooter}>
          <label className={styles.searchForMissingMovieLabelContainer}>
            <span className={styles.searchForMissingMovieLabel}>
              Start search for missing movie
            </span>

            <CheckInput
              containerClassName={styles.searchForMissingMovieContainer}
              className={styles.searchForMissingMovieInput}
              name="searchForMovie"
              value={this.state.searchForMovie}
              onChange={this.onSearchForMissingMovieChange}
            />
          </label>

          <SpinnerButton
            className={styles.addButton}
            kind={kinds.SUCCESS}
            isSpinning={isAdding}
            onPress={this.onAddMoviePress}
          >
            Add {title}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNewMovieModalContent.propTypes = {
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  minimumAvailability: PropTypes.object.isRequired,
  folder: PropTypes.string.isRequired,
  tags: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isWindows: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onAddMoviePress: PropTypes.func.isRequired
};

export default AddNewMovieModalContent;
