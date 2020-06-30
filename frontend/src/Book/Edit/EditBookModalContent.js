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

class EditBookModalContent extends Component {

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
      authorName,
      statistics,
      item,
      isSaving,
      onInputChange,
      onModalClose,
      ...otherProps
    } = this.props;

    const {
      monitored,
      anyEditionOk,
      editions
    } = item;

    const hasFile = statistics ? statistics.bookFileCount : 0;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Edit - {authorName} - {title}
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
                helpText="Readarr will search for and download book"
                {...monitored}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Automatically Switch Edition</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="anyEditionOk"
                helpText="Readarr will automatically switch to the edition best matching downloaded files"
                {...anyEditionOk}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Edition</FormLabel>

              <FormInputGroup
                type={inputTypes.BOOK_EDITION_SELECT}
                name="editions"
                helpText="Change edition for this book"
                isDisabled={anyEditionOk.value && hasFile}
                bookEditions={editions}
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

EditBookModalContent.propTypes = {
  bookId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  authorName: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  item: PropTypes.object.isRequired,
  isSaving: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditBookModalContent;
