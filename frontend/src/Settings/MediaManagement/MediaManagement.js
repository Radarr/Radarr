import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, sizes } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FieldSet from 'Components/FieldSet';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import RootFoldersConnector from './RootFolder/RootFoldersConnector';
import NamingConnector from './Naming/NamingConnector';

const rescanAfterRefreshOptions = [
  { key: 'always', value: 'Always' },
  { key: 'afterManual', value: 'After Manual Refresh' },
  { key: 'never', value: 'Never' }
];

const allowFingerprintingOptions = [
  { key: 'allFiles', value: 'Always' },
  { key: 'newFiles', value: 'For new imports only' },
  { key: 'never', value: 'Never' }
];

const downloadPropersAndRepacksOptions = [
  { key: 'preferAndUpgrade', value: 'Prefer and Upgrade' },
  { key: 'doNotUpgrade', value: 'Do not Upgrade Automatically' },
  { key: 'doNotPrefer', value: 'Do not Prefer' }
];

const fileDateOptions = [
  { key: 'none', value: 'None' },
  { key: 'albumReleaseDate', value: 'Album Release Date' }
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

        <PageContentBodyConnector>
          <RootFoldersConnector />
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
                        <FormLabel>Create empty artist folders</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="createEmptyArtistFolders"
                          helpText="Create missing artist folders during disk scan"
                          onChange={onInputChange}
                          {...settings.createEmptyArtistFolders}
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
                          name="deleteEmptyFolders"
                          helpText="Delete empty artist and album folders during disk scan and when track files are deleted"
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
                              helpText="Use when Lidarr is unable to detect free space from your artist root folder"
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
                          helpTextWarning="Occasionally, file locks may prevent renaming files that are being seeded. You may temporarily disable seeding and use Lidarr's rename function as a work around."
                          onChange={onInputChange}
                          {...settings.copyUsingHardlinks}
                        />
                      </FormGroup>

                      <FormGroup size={sizes.MEDIUM}>
                        <FormLabel>Import Extra Files</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="importExtraFiles"
                          helpText="Import matching extra files (subtitles, nfo, etc) after importing an track file"
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
                  legend="File Management"
                >
                  <FormGroup size={sizes.MEDIUM}>
                    <FormLabel>Ignore Deleted Tracks</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="autoUnmonitorPreviouslyDownloadedTracks"
                      helpText="Tracks deleted from disk are automatically unmonitored in Lidarr"
                      onChange={onInputChange}
                      {...settings.autoUnmonitorPreviouslyDownloadedTracks}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>Propers and Repacks</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="downloadPropersAndRepacks"
                      helpTexts={[
                        'Whether or not to automatically upgrade to Propers/Repacks',
                        'Use \'Do not Prefer\' to sort by preferred word score over propers/repacks'
                      ]}
                      helpTextWarning={
                        settings.downloadPropersAndRepacks.value === 'doNotPrefer' ?
                          'Use preferred words for automatic upgrades to propers/repacks' :
                          undefined
                      }
                      values={downloadPropersAndRepacksOptions}
                      onChange={onInputChange}
                      {...settings.downloadPropersAndRepacks}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>Watch Root Folders for file changes</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="watchLibraryForChanges"
                      helpText="Rescan automatically when files change in a root folder"
                      onChange={onInputChange}
                      {...settings.watchLibraryForChanges}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>Rescan Artist Folder after Refresh</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="rescanAfterRefresh"
                      helpText="Rescan the artist folder after refreshing the artist"
                      helpTextWarning="Lidarr will not automatically detect changes to files when not set to 'Always'"
                      values={rescanAfterRefreshOptions}
                      onChange={onInputChange}
                      {...settings.rescanAfterRefresh}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>Allow Fingerprinting</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="allowFingerprinting"
                      helpText="Use fingerprinting to improve accuracy of track matching"
                      helpTextWarning="This requires Lidarr to read parts of the file which will slow down scans and may cause high disk or network activity."
                      values={allowFingerprintingOptions}
                      onChange={onInputChange}
                      {...settings.allowFingerprinting}
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
                      helpText="Track files will go here when deleted instead of being permanently deleted"
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
                          helpText="Should chmod/chown be run when files are imported/renamed?"
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
                          helpText="Octal, applied to media files when imported/renamed by Lidarr"
                          onChange={onInputChange}
                          {...settings.fileChmod}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>Folder chmod mode</FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT}
                          name="folderChmod"
                          helpText="Octal, applied to artist/album folders created by Lidarr"
                          values={fileDateOptions}
                          onChange={onInputChange}
                          {...settings.folderChmod}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>chown User</FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT}
                          name="chownUser"
                          helpText="Username or uid. Use uid for remote file systems."
                          values={fileDateOptions}
                          onChange={onInputChange}
                          {...settings.chownUser}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>chown Group</FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT}
                          name="chownGroup"
                          helpText="Group name or gid. Use gid for remote file systems."
                          values={fileDateOptions}
                          onChange={onInputChange}
                          {...settings.chownGroup}
                        />
                      </FormGroup>
                    </FieldSet>
                }
              </Form>
          }
        </PageContentBodyConnector>
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
