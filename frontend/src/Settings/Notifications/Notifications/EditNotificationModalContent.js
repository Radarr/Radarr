import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import Alert from 'Components/Alert';
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
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import styles from './EditNotificationModalContent.css';

function EditNotificationModalContent(props) {
  const {
    advancedSettings,
    isFetching,
    error,
    isSaving,
    isTesting,
    saveError,
    item,
    onInputChange,
    onFieldChange,
    onModalClose,
    onSavePress,
    onTestPress,
    onDeleteNotificationPress,
    ...otherProps
  } = props;

  const {
    id,
    name,
    onGrab,
    onDownload,
    onAlbumDownload,
    onUpgrade,
    onRename,
    supportsOnGrab,
    supportsOnDownload,
    supportsOnAlbumDownload,
    supportsOnUpgrade,
    supportsOnRename,
    tags,
    fields,
    message
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Notification' : 'Add Notification'}
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to add a new notification, please try again.</div>
        }

        {
          !isFetching && !error &&
            <Form
              {...otherProps}
            >
              {
                !!message &&
                  <Alert
                    className={styles.message}
                    kind={message.value.type}
                  >
                    {message.value.message}
                  </Alert>
              }

              <FormGroup>
                <FormLabel>Name</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="name"
                  {...name}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>On Grab</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onGrab"
                  helpText="Be notified when albums are available for download and has been sent to a download client"
                  isDisabled={!supportsOnGrab.value}
                  {...onGrab}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>On Album Import</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onAlbumDownload"
                  helpText="Be notified when complete albums are successfully imported"
                  isDisabled={!supportsOnAlbumDownload.value}
                  {...onAlbumDownload}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>On Track Import</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onDownload"
                  helpText="Be notified when track files are successfully imported"
                  isDisabled={!supportsOnDownload.value}
                  {...onDownload}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                onDownload.value &&
                  <FormGroup>
                    <FormLabel>On Track Upgrade</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="onUpgrade"
                      helpText="Be notified when tracks are upgraded to a better quality"
                      isDisabled={!supportsOnUpgrade.value}
                      {...onUpgrade}
                      onChange={onInputChange}
                    />
                  </FormGroup>
              }

              <FormGroup>
                <FormLabel>On Rename</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onRename"
                  helpText="Be notified when tracks are renamed"
                  isDisabled={!supportsOnRename.value}
                  {...onRename}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Tags</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  helpText="Only send notifications for artist with at least one matching tag"
                  {...tags}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                fields.map((field) => {
                  return (
                    <ProviderFieldFormGroup
                      key={field.name}
                      advancedSettings={advancedSettings}
                      provider="notification"
                      providerData={item}
                      {...field}
                      onChange={onFieldChange}
                    />
                  );
                })
              }

            </Form>
        }
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteNotificationPress}
            >
              Delete
            </Button>
        }

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={onTestPress}
        >
          Test
        </SpinnerErrorButton>

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

EditNotificationModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  isTesting: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onDeleteNotificationPress: PropTypes.func
};

export default EditNotificationModalContent;
