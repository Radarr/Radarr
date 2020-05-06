import PropTypes from 'prop-types';
import React from 'react';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
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
import Popover from 'Components/Tooltip/Popover';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import styles from './EditImportListModalContent.css';

function ImportListMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title="None"
        data="Do not monitor authors or books"
      />

      <DescriptionListItem
        title="Specific Book"
        data="Monitor authors but only monitor books explicitly included in the list"
      />

      <DescriptionListItem
        title="All Author Books"
        data="Monitor authors and all books for each author included on the import list"
      />
    </DescriptionList>
  );
}

function EditImportListModalContent(props) {

  const monitorOptions = [
    { key: 'none', value: 'None' },
    { key: 'specificAlbum', value: 'Specific Book' },
    { key: 'entireArtist', value: 'All Author Books' }
  ];

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
    onDeleteImportListPress,
    showMetadataProfile,
    ...otherProps
  } = props;

  const {
    id,
    name,
    enableAutomaticAdd,
    shouldMonitor,
    rootFolderPath,
    qualityProfileId,
    metadataProfileId,
    tags,
    fields
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit List' : 'Add List'}
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
            <Form {...otherProps}>
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
                <FormLabel>Enable Automatic Add</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableAutomaticAdd"
                  helpText={'Add author/books to Readarr when syncs are performed via the UI or by Readarr'}
                  {...enableAutomaticAdd}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  Monitor

                  <Popover
                    anchor={
                      <Icon
                        className={styles.labelIcon}
                        name={icons.INFO}
                      />
                    }
                    title="Monitoring Options"
                    body={<ImportListMonitoringOptionsPopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="shouldMonitor"
                  values={monitorOptions}
                  helpText={'Monitor authors and books added from this list'}
                  {...shouldMonitor}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Root Folder</FormLabel>

                <FormInputGroup
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  helpText={'Root Folder list items will be added to'}
                  {...rootFolderPath}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Quality Profile</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  helpText={'Quality Profile list items should be added with'}
                  {...qualityProfileId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup className={showMetadataProfile ? undefined : styles.hideMetadataProfile}>
                <FormLabel>Metadata Profile</FormLabel>

                <FormInputGroup
                  type={inputTypes.METADATA_PROFILE_SELECT}
                  name="metadataProfileId"
                  helpText={'Metadata Profile list items should be added with'}
                  {...metadataProfileId}
                  includeNone={true}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Readarr Tags</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  helpText="Add authors from this list with these tags"
                  {...tags}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                !!fields && !!fields.length &&
                <div>
                  {
                    fields.map((field) => {
                      return (
                        <ProviderFieldFormGroup
                          key={field.name}
                          advancedSettings={advancedSettings}
                          provider="importList"
                          providerData={item}
                          section="settings.importLists"
                          {...field}
                          onChange={onFieldChange}
                        />
                      );
                    })
                  }
                </div>
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

EditImportListModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  isTesting: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  showMetadataProfile: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onDeleteImportListPress: PropTypes.func
};

export default EditImportListModalContent;
