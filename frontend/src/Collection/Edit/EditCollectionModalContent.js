import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import MoviePoster from 'Movie/MoviePoster';
import translate from 'Utilities/String/translate';
import styles from './EditCollectionModalContent.css';

class EditCollectionModalContent extends Component {

  //
  // Listeners

  onSavePress = () => {
    const {
      onSavePress
    } = this.props;

    onSavePress(false);
  };

  //
  // Render

  render() {
    const {
      title,
      images,
      overview,
      item,
      isSaving,
      onInputChange,
      onModalClose,
      isSmallScreen,
      ...otherProps
    } = this.props;

    const {
      monitored,
      qualityProfileId,
      minimumAvailability,
      // Id,
      rootFolderPath,
      searchOnAdd
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('Edit')} - {title}
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

              <Form
                {...otherProps}
              >
                <FormGroup>
                  <FormLabel>{translate('Monitored')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="monitored"
                    helpText={translate('MonitoredCollectionHelpText')}
                    {...monitored}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('MinimumAvailability')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.AVAILABILITY_SELECT}
                    name="minimumAvailability"
                    {...minimumAvailability}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('QualityProfile')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.QUALITY_PROFILE_SELECT}
                    name="qualityProfileId"
                    {...qualityProfileId}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Folder')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.ROOT_FOLDER_SELECT}
                    name="rootFolderPath"
                    {...rootFolderPath}
                    includeMissingValue={true}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('SearchOnAdd')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="searchOnAdd"
                    helpText={translate('SearchOnAddCollectionHelpText')}
                    {...searchOnAdd}
                    onChange={onInputChange}
                  />
                </FormGroup>
              </Form>
            </div>
          </div>
        </ModalBody>

        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            {translate('Cancel')}
          </Button>

          <SpinnerButton
            isSpinning={isSaving}
            onPress={this.onSavePress}
          >
            {translate('Save')}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

EditCollectionModalContent.propTypes = {
  collectionId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  overview: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  item: PropTypes.object.isRequired,
  isSaving: PropTypes.bool.isRequired,
  isPathChanging: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditCollectionModalContent;
