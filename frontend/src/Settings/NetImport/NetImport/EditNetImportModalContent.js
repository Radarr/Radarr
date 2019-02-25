import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
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
import styles from './EditNetImportModalContent.css';

function EditNetImportModalContent(props) {
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
    onDeleteNetImportPress,
    ...otherProps
  } = props;

  const {
    id,
    implementationName,
    name,
    enabled,
    enableAuto,
    shouldMonitor,
    qualityProfileId,
    rootFolderPath,
    fields
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {`${id ? 'Edit' : 'Add'} List - ${implementationName}`}
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to add a new list, please try again.</div>
        }

        {
          !isFetching && !error &&
            <Form
              {...otherProps}
            >
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
                <FormLabel>Enable</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enabled"
                  helpText="Enable this list for use in Radarr"
                  {...enabled}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Enable Automatic Add</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableAuto"
                  helpText="If enabled, Movies will be automatically added to Radarr from this list"
                  {...enableAuto}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Add Movies Monitored</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="shouldMonitor"
                  helpText="If enabled, movies added by this list are added and monitored"
                  {...shouldMonitor}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Quality Profile</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  {...qualityProfileId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Folder</FormLabel>

                <FormInputGroup
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  {...rootFolderPath}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                fields.map((field) => {
                  return (
                    <ProviderFieldFormGroup
                      key={field.name}
                      advancedSettings={advancedSettings}
                      provider="netImport"
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
              onPress={onDeleteNetImportPress}
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

EditNetImportModalContent.propTypes = {
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
  onDeleteNetImportPress: PropTypes.func
};

export default EditNetImportModalContent;
