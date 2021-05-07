import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EditImportListModalContent.css';

function EditImportListModalContent(props) {
  const {
    advancedSettings,
    isFetching,
    error,
    rootFolderError,
    isSaving,
    isTesting,
    saveError,
    item,
    onInputChange,
    onFieldChange,
    onModalClose,
    onSavePress,
    onTestPress,
    onDeleteImportListPress,
    ...otherProps
  } = props;

  const {
    id,
    implementationName,
    name,
    enabled,
    enableAuto,
    shouldMonitor,
    minimumAvailability,
    qualityProfileId,
    rootFolderPath,
    searchOnAdd,
    tags,
    fields,
    message
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {`${id ? translate('Edit') : translate('Add')} ${translate('List')} - ${implementationName}`}
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && (!!error || !!rootFolderError) &&
            <div>
              {translate('UnableToAddANewListPleaseTryAgain')}
            </div>
        }

        {
          !isFetching && !error && !rootFolderError &&
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
                <FormLabel>{translate('Name')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="name"
                  {...name}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('Enable')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enabled"
                  helpText={translate('EnabledHelpText')}
                  {...enabled}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('EnableAutomaticAdd')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableAuto"
                  helpText={translate('EnableAutoHelpText')}
                  {...enableAuto}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('AddMoviesMonitored')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="shouldMonitor"
                  helpText={translate('ShouldMonitorHelpText')}
                  {...shouldMonitor}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                shouldMonitor &&
                  <FormGroup>
                    <FormLabel>{translate('SearchOnAdd')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="searchOnAdd"
                      helpText={translate('SearchOnAddHelpText')}
                      {...searchOnAdd}
                      onChange={onInputChange}
                    />
                  </FormGroup>
              }

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
                <FormLabel>{translate('RadarrTags')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  helpText={translate('ListTagsHelpText')}
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
                      provider="importList"
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
              onPress={onDeleteImportListPress}
            >
              {translate('Delete')}
            </Button>
        }

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={onTestPress}
        >
          {translate('Test')}
        </SpinnerErrorButton>

        <Button
          onPress={onModalClose}
        >
          {translate('Cancel')}
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditImportListModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  rootFolderError: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  isTesting: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onDeleteImportListPress: PropTypes.func
};

export default EditImportListModalContent;
