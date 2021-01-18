import PropTypes from 'prop-types';
import React from 'react';
import AuthorMetadataProfilePopoverContent from 'AddAuthor/AuthorMetadataProfilePopoverContent';
import AuthorMonitoringOptionsPopoverContent from 'AddAuthor/AuthorMonitoringOptionsPopoverContent';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import styles from './EditRootFolderModalContent.css';

function EditRootFolderModalContent(props) {

  const {
    advancedSettings,
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    onInputChange,
    onModalClose,
    onSavePress,
    onDeleteRootFolderPress,
    showMetadataProfile,
    ...otherProps
  } = props;

  const {
    id,
    name,
    path,
    defaultQualityProfileId,
    defaultMetadataProfileId,
    defaultMonitorOption,
    defaultTags,
    isCalibreLibrary,
    host,
    port,
    urlBase,
    username,
    password,
    library,
    outputFormat,
    outputProfile,
    useSsl
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Root Folder' : 'Add Root Folder'}
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to add a new root folder, please try again.</div>
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
                <FormLabel>Path</FormLabel>

                <FormInputGroup
                  type={id ? inputTypes.TEXT : inputTypes.PATH}
                  readOnly={!!id}
                  name="path"
                  helpText="Root Folder containing your book library"
                  helpTextWarning="This must be different to the directory where your download client puts files"
                  {...path}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Use Calibre</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="isCalibreLibrary"
                  helpText="Use calibre content server to manipulate library"
                  {...isCalibreLibrary}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                isCalibreLibrary !== undefined && isCalibreLibrary.value &&
                  <div>
                    <FormGroup>
                      <FormLabel>Calibre Host</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="host"
                        helpText="Calibre content server host"
                        {...host}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Calibre Port</FormLabel>

                      <FormInputGroup
                        type={inputTypes.NUMBER}
                        name="port"
                        helpText="Calibre content server port"
                        {...port}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup
                      advancedSettings={advancedSettings}
                      isAdvanced={true}
                    >
                      <FormLabel>Calibre Url Base</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="urlBase"
                        helpText="Adds a prefix to the calibre url, e.g. http://[host]:[port]/[urlBase]"
                        {...urlBase}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Calibre Username</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="username"
                        helpText="Calibre content server username"
                        {...username}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Calibre Password</FormLabel>

                      <FormInputGroup
                        type={inputTypes.PASSWORD}
                        name="password"
                        helpText="Calibre content server password"
                        {...password}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Calibre Library</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="library"
                        helpText="Calibre content server library name.  Leave blank for default."
                        {...library}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Convert to format</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="outputFormat"
                        helpText="Optionally ask calibre to convert to other formats on import. Comma separated list."
                        {...outputFormat}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Calibre Output Profile</FormLabel>

                      <FormInputGroup
                        type={inputTypes.NUMBER}
                        name="outputProfile"
                        helpText="Output profile for conversion"
                        {...outputProfile}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Use SSL</FormLabel>

                      <FormInputGroup
                        type={inputTypes.CHECK}
                        name="useSsl"
                        helpText="Use SSL to connect to calibre content server"
                        {...useSsl}
                        onChange={onInputChange}
                      />
                    </FormGroup>
                  </div>
              }

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
                    body={<AuthorMonitoringOptionsPopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.MONITOR_BOOKS_SELECT}
                  name="defaultMonitorOption"
                  onChange={onInputChange}
                  {...defaultMonitorOption}
                  helpText="Default Monitoring Options for books by authors detected in this folder"
                />

              </FormGroup>

              <FormGroup>
                <FormLabel>Quality Profile</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="defaultQualityProfileId"
                  helpText="Default Quality Profile for authors detected in this folder"
                  {...defaultQualityProfileId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup className={showMetadataProfile ? undefined : styles.hideMetadataProfile}>
                <FormLabel>
                  Metadata Profile
                  <Popover
                    anchor={
                      <Icon
                        className={styles.labelIcon}
                        name={icons.INFO}
                      />
                    }
                    title="Metadata Profile"
                    body={<AuthorMetadataProfilePopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.METADATA_PROFILE_SELECT}
                  name="defaultMetadataProfileId"
                  helpText="Default Metadata Profile for authors detected in this folder"
                  {...defaultMetadataProfileId}
                  includeNone={true}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Default Readarr Tags</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="defaultTags"
                  helpText="Default Readarr Tags for authors detected in this folder"
                  {...defaultTags}
                  onChange={onInputChange}
                />
              </FormGroup>

            </Form>
        }
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteRootFolderPress}
            >
              Delete
            </Button>
        }

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

EditRootFolderModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  showMetadataProfile: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onDeleteRootFolderPress: PropTypes.func
};

export default EditRootFolderModalContent;
