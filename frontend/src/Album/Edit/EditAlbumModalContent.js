import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

class EditAlbumModalContent extends Component {

  //
  // Listeners

  onSavePress = () => {
    const {
      onSavePress
    } = this.props;

    onSavePress(false);

  }

  //
  // Render

  render() {
    const {
      title,
      artistName,
      albumType,
      statistics,
      item,
      isSaving,
      onInputChange,
      onModalClose,
      ...otherProps
    } = this.props;

    const {
      monitored,
      anyReleaseOk,
      releases
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Edit - {artistName} - {title} [{albumType}]
        </ModalHeader>

        <ModalBody>
          <Form
            {...otherProps}
          >
            <FormGroup>
              <FormLabel>Monitored</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="monitored"
                helpText="Lidarr will search for and download album"
                {...monitored}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Automatically Switch Release</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="anyReleaseOk"
                helpText="Lidarr will automatically switch to the release best matching downloaded tracks"
                {...anyReleaseOk}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel> Release</FormLabel>

              <FormInputGroup
                type={inputTypes.ALBUM_RELEASE_SELECT}
                name="releases"
                helpText="Change release for this album"
                isDisabled={anyReleaseOk.value && statistics.trackFileCount > 0}
                albumReleases={releases}
                onChange={onInputChange}
              />
            </FormGroup>

          </Form>
        </ModalBody>
        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Cancel
          </Button>

          <SpinnerButton
            isSpinning={isSaving}
            onPress={this.onSavePress}
          >
            Save
          </SpinnerButton>
        </ModalFooter>

      </ModalContent>
    );
  }
}

EditAlbumModalContent.propTypes = {
  albumId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  albumType: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  item: PropTypes.object.isRequired,
  isSaving: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditAlbumModalContent;
