import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, sizes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import styles from './EditCustomFormatModalContent.css';

class EditCustomFormatModalContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      isSaving,
      saveError,
      item,
      onInputChange,
      onSavePress,
      onModalClose,
      ...otherProps
    } = this.props;

    const {
      id,
      name,
      formatTags
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>

        <ModalHeader>
          {id ? 'Edit Custom Format' : 'Add Custom Format'}
        </ModalHeader>

        <ModalBody>
          <div>
            {
              isFetching &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <div>Unable to add a new custom format, please try again.</div>
            }

            {
              !isFetching && !error &&
                <Form
                  {...otherProps}
                >
                  <div className={styles.formGroupsContainer}>
                    <div className={styles.formGroupWrapper}>
                      <FormGroup size={sizes.EXTRA_SMALL}>
                        <FormLabel size={sizes.SMALL}>
                          Name
                        </FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT}
                          name="name"
                          {...name}
                          onChange={onInputChange}
                        />
                      </FormGroup>

                      <FormGroup size={sizes.EXTRA_SMALL}>
                        <FormLabel size={sizes.SMALL}>
                          Format Tags
                        </FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT_TAG}
                          name="formatTags"
                          {...formatTags}
                          onChange={onInputChange}
                        />
                      </FormGroup>
                    </div>
                  </div>
                </Form>

            }
          </div>
        </ModalBody>
        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Cancel
          </Button>

          <SpinnerErrorButton
            isSpinning={isSaving}
            error={saveError}
            onPress={onSavePress}
          >
            Save
          </SpinnerErrorButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

EditCustomFormatModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onContentHeightChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditCustomFormatModalContent;
