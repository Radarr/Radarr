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
import translate from 'Utilities/String/translate';
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
    implementationName,
    name,
    onGrab,
    onDownload,
    onUpgrade,
    onRename,
    onHealthIssue,
    supportsOnGrab,
    supportsOnDownload,
    supportsOnUpgrade,
    supportsOnRename,
    supportsOnHealthIssue,
    includeHealthWarnings,
    tags,
    fields,
    message
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {`${id ? 'Edit' : 'Add'} Connection - ${implementationName}`}
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
            <Form {...otherProps}>
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
                  helpText="Be notified when movies are available for download and has been sent to a download client"
                  isDisabled={!supportsOnGrab.value}
                  {...onGrab}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>On Import</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onDownload"
                  helpText="Be notified when movies are successfully imported"
                  isDisabled={!supportsOnDownload.value}
                  {...onDownload}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                onDownload.value &&
                  <FormGroup>
                    <FormLabel>On Upgrade</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="onUpgrade"
                      helpText="Be notified when movies are upgraded to a better quality"
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
                  helpText="Be notified when movies are renamed"
                  isDisabled={!supportsOnRename.value}
                  {...onRename}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>On Health Issue</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onHealthIssue"
                  helpText="Be notified on health check failures"
                  isDisabled={!supportsOnHealthIssue.value}
                  {...onHealthIssue}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                onHealthIssue.value &&
                  <FormGroup>
                    <FormLabel>Include Health Warnings</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="includeHealthWarnings"
                      helpText="Be notified on health warnings in addition to errors"
                      isDisabled={!supportsOnHealthIssue.value}
                      {...includeHealthWarnings}
                      onChange={onInputChange}
                    />
                  </FormGroup>
              }

              <FormGroup>
                <FormLabel>Tags</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  helpText="Only send notifications for movies with at least one matching tag"
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
                      section="settings.notifications"
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
              {translate('Delete')}
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
