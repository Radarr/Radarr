import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { inputTypes, sizes } from 'Helpers/Props';
import RootFoldersConnector from 'RootFolder/RootFoldersConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import NamingConnector from './Naming/NamingConnector';
import AddRootFolderConnector from './RootFolder/AddRootFolderConnector';

const rescanAfterRefreshOptions = [
  { key: 'always', value: 'Always' },
  { key: 'afterManual', value: 'After Manual Refresh' },
  { key: 'never', value: 'Never' }
];

const fileDateOptions = [
  { key: 'none', value: 'None' },
  { key: 'cinemas', value: 'In Cinemas Date' },
  { key: 'release', value: 'Physical Release Date' }
];

class MediaManagement extends Component {

  //
  // Render

  render() {
    const {
      advancedSettings,
      isFetching,
      error,
      settings,
      hasSettings,
      isWindows,
      onInputChange,
      onSavePress,
      ...otherProps
    } = this.props;

    return (
      <PageContent title="Media Management Settings">
        <SettingsToolbarConnector
          advancedSettings={advancedSettings}
          {...otherProps}
          onSavePress={onSavePress}
        />

        <PageContentBody>
          <NamingConnector />

          {
            isFetching &&
              <FieldSet legend="Naming Settings">
                <LoadingIndicator />
              </FieldSet>
          }

          {
            !isFetching && error &&
              <FieldSet legend="Naming Settings">
                <div>Unable to load Media Management settings</div>
              </FieldSet>
          }

          {
            hasSettings && !isFetching && !error &&
              <Form
                id="mediaManagementSettings"
                {...otherProps}
              >
                {
                  advancedSettings &&
                    <FieldSet legend="Folders">
                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>Create empty movie folders</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          isDisabled={settings.deleteEmptyFolders.value && !settings.createEmptyMovieFolders.value}
                          name="createEmptyMovieFolders"
                          helpText="Create missing movie folders during disk scan"
                          onChange={onInputChange}
                          {...settings.createEmptyMovieFolders}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>Delete empty folders</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          isDisabled={settings.createEmptyMovieFolders.value && !settings.deleteEmptyFolders.value}
                          name="deleteEmptyFolders"
                          helpText="Delete empty movie folders during disk scan and when movie files are deleted"
                          onChange={onInputChange}
                          {...settings.deleteEmptyFolders}
                        />
                      </FormGroup>
                    </FieldSet>
                }

                {
                  advancedSettings &&
                    <FieldSet
                      legend="Importing"
                    >
                      {
                        !isWindows &&
                          <FormGroup
                            advancedSettings={advancedSettings}
                            isAdvanced={true}
                            size={sizes.MEDIUM}
                          >
                            <FormLabel>Skip Free Space Check</FormLabel>

                            <FormInputGroup
                              type={inputTypes.CHECK}
                              name="skipFreeSpaceCheckWhenImporting"
                              helpText="Use when Radarr is unable to detect free space from your movie root folder"
                              onChange={onInputChange}
                              {...settings.skipFreeSpaceCheckWhenImporting}
                            />
                          </FormGroup>
                      }

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>Minimum Free Space</FormLabel>

                        <FormInputGroup
                          type={inputTypes.NUMBER}
                          unit='MB'
                          name="minimumFreeSpaceWhenImporting"
                          helpText="Prevent import if it would leave less than this amount of disk space available"
                          onChange={onInputChange}
                          {...settings.minimumFreeSpaceWhenImporting}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>Use Hardlinks instead of Copy</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="copyUsingHardlinks"
                          helpText="Use Hardlinks when trying to copy files from torrents that are still being seeded"
                          helpTextWarning="Occasionally, file locks may prevent renaming files that are being seeded. You may temporarily disable seeding and use Radarr's rename function as a work around."
                          onChange={onInputChange}
                          {...settings.copyUsingHardlinks}
                        />
                      </FormGroup>

                      <FormGroup size={sizes.MEDIUM}>
                        <FormLabel>Import Extra Files</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="importExtraFiles"
                          helpText="Import matching extra files (subtitles, nfo, etc) after importing an movie file"
                          onChange={onInputChange}
                          {...settings.importExtraFiles}
                        />
                      </FormGroup>

                      {
                        settings.importExtraFiles.value &&
                          <FormGroup
                            advancedSettings={advancedSettings}
                            isAdvanced={true}
                          >
                            <FormLabel>Import Extra Files</FormLabel>

                            <FormInputGroup
                              type={inputTypes.TEXT}
                              name="extraFileExtensions"
                              helpTexts={[
                                'Comma separated list of extra files to import (.nfo will be imported as .nfo-orig)',
                                'Examples: ".sub, .nfo" or "sub,nfo"'
                              ]}
                              onChange={onInputChange}
                              {...settings.extraFileExtensions}
                            />
                          </FormGroup>
                      }
                    </FieldSet>
                }

                <FieldSet
                  legend={translate('FileManagement')}
                >
                  <FormGroup size={sizes.MEDIUM}>
                    <FormLabel>Ignore Deleted Movies</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="autoUnmonitorPreviouslyDownloadedMovies"
                      helpText="Movies deleted from disk are automatically unmonitored in Radarr"
                      onChange={onInputChange}
                      {...settings.autoUnmonitorPreviouslyDownloadedMovies}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>Download Propers</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="autoDownloadPropers"
                      helpText="Should Radarr automatically upgrade to propers when available?"
                      onChange={onInputChange}
                      {...settings.autoDownloadPropers}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>Analyse video files</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="enableMediaInfo"
                      helpText="Extract video information such as resolution, runtime and codec information from files. This requires Radarr to read parts of the file which may cause high disk or network activity during scans."
                      onChange={onInputChange}
                      {...settings.enableMediaInfo}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>Rescan Movie Folder after Refresh</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="rescanAfterRefresh"
                      helpText="Rescan the movie folder after refreshing the movie"
                      helpTextWarning="Radarr will not automatically detect changes to files when not set to 'Always'"
                      values={rescanAfterRefreshOptions}
                      onChange={onInputChange}
                      {...settings.rescanAfterRefresh}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>Change File Date</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="fileDate"
                      helpText="Change file date on import/rescan"
                      values={fileDateOptions}
                      onChange={onInputChange}
                      {...settings.fileDate}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>Recycling Bin</FormLabel>

                    <FormInputGroup
                      type={inputTypes.PATH}
                      name="recycleBin"
                      helpText="Movie files will go here when deleted instead of being permanently deleted"
                      onChange={onInputChange}
                      {...settings.recycleBin}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>Recycling Bin Cleanup</FormLabel>

                    <FormInputGroup
                      type={inputTypes.NUMBER}
                      name="recycleBinCleanupDays"
                      helpText="Set to 0 to disable automatic cleanup"
                      helpTextWarning="Files in the recycle bin older than the selected number of days will be cleaned up automatically"
                      min={0}
                      onChange={onInputChange}
                      {...settings.recycleBinCleanupDays}
                    />
                  </FormGroup>
                </FieldSet>

                {
                  advancedSettings && !isWindows &&
                    <FieldSet
                      legend="Permissions"
                    >
                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>Set Permissions</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="setPermissionsLinux"
                          helpText="Should chmod be run when files are imported/renamed?"
                          helpTextWarning="If you're unsure what these settings do, do not alter them."
                          onChange={onInputChange}
                          {...settings.setPermissionsLinux}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>File chmod mode</FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT}
                          name="fileChmod"
                          helpTexts={[
                            'Octal, applied to media files when imported/renamed by Radarr',
                            'The same mode is applied to movie/sub folders with the execute bit added, e.g., 0644 becomes 0755'
                          ]}
                          onChange={onInputChange}
                          {...settings.fileChmod}
                        />
                      </FormGroup>
                    </FieldSet>
                }
              </Form>
          }

          <FieldSet legend={translate('RootFolders')}>
            <RootFoldersConnector />
            <AddRootFolderConnector />
          </FieldSet>
        </PageContentBody>
      </PageContent>
    );
  }

}

MediaManagement.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  isWindows: PropTypes.bool.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default MediaManagement;
