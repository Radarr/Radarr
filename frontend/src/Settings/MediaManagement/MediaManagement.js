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
  { key: 'always', value: translate('Always') },
  { key: 'afterManual', value: translate('AfterManualRefresh') },
  { key: 'never', value: translate('Never') }
];

const downloadPropersAndRepacksOptions = [
  { key: 'preferAndUpgrade', value: translate('PreferAndUpgrade') },
  { key: 'doNotUpgrade', value: translate('DoNotUpgradeAutomatically') },
  { key: 'doNotPrefer', value: translate('DoNotPrefer') }
];

const fileDateOptions = [
  { key: 'none', value: translate('None') },
  { key: 'cinemas', value: translate('InCinemasDate') },
  { key: 'release', value: translate('PhysicalReleaseDate') }
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
      <PageContent title={translate('MediaManagementSettings')}>
        <SettingsToolbarConnector
          advancedSettings={advancedSettings}
          {...otherProps}
          onSavePress={onSavePress}
        />

        <PageContentBody>
          <NamingConnector />

          {
            isFetching ?
              <FieldSet legend={translate('NamingSettings')}>
                <LoadingIndicator />
              </FieldSet> : null
          }

          {
            !isFetching && error ?
              <FieldSet legend={translate('NamingSettings')}>
                <div>
                  {translate('UnableToLoadMediaManagementSettings')}
                </div>
              </FieldSet> : null
          }

          {
            hasSettings && !isFetching && !error ?
              <Form
                id="mediaManagementSettings"
                {...otherProps}
              >
                {
                  advancedSettings ?
                    <FieldSet legend={translate('Folders')}>
                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>{translate('CreateEmptyMovieFolders')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          isDisabled={settings.deleteEmptyFolders.value && !settings.createEmptyMovieFolders.value}
                          name="createEmptyMovieFolders"
                          helpText={translate('CreateEmptyMovieFoldersHelpText')}
                          onChange={onInputChange}
                          {...settings.createEmptyMovieFolders}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>{translate('DeleteEmptyFolders')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          isDisabled={settings.createEmptyMovieFolders.value && !settings.deleteEmptyFolders.value}
                          name="deleteEmptyFolders"
                          helpText={translate('DeleteEmptyFoldersHelpText')}
                          onChange={onInputChange}
                          {...settings.deleteEmptyFolders}
                        />
                      </FormGroup>
                    </FieldSet> : null
                }

                {
                  advancedSettings ?
                    <FieldSet
                      legend={translate('Importing')}
                    >
                      {
                        !isWindows &&
                          <FormGroup
                            advancedSettings={advancedSettings}
                            isAdvanced={true}
                            size={sizes.MEDIUM}
                          >
                            <FormLabel>{translate('SkipFreeSpaceCheck')}</FormLabel>

                            <FormInputGroup
                              type={inputTypes.CHECK}
                              name="skipFreeSpaceCheckWhenImporting"
                              helpText={translate('SkipFreeSpaceCheckWhenImportingHelpText')}
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
                        <FormLabel>{translate('MinimumFreeSpace')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.NUMBER}
                          unit='MB'
                          name="minimumFreeSpaceWhenImporting"
                          helpText={translate('MinimumFreeSpaceWhenImportingHelpText')}
                          onChange={onInputChange}
                          {...settings.minimumFreeSpaceWhenImporting}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>{translate('UseHardlinksInsteadOfCopy')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="copyUsingHardlinks"
                          helpText={translate('CopyUsingHardlinksHelpText')}
                          helpTextWarning={translate('CopyUsingHardlinksHelpTextWarning')}
                          onChange={onInputChange}
                          {...settings.copyUsingHardlinks}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>{translate('ImportUsingScript')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="useScriptImport"
                          helpText={translate('UseScriptImportHelpText')}
                          onChange={onInputChange}
                          {...settings.useScriptImport}
                        />
                      </FormGroup>

                      {
                        settings.useScriptImport.value ?
                          <FormGroup
                            advancedSettings={advancedSettings}
                            isAdvanced={true}
                          >
                            <FormLabel>{translate('ImportScriptPath')}</FormLabel>

                            <FormInputGroup
                              type={inputTypes.PATH}
                              includeFiles={true}
                              name="scriptImportPath"
                              helpText={translate('ScriptImportPathHelpText')}
                              onChange={onInputChange}
                              {...settings.scriptImportPath}
                            />
                          </FormGroup> : null
                      }

                      <FormGroup size={sizes.MEDIUM}>
                        <FormLabel>{translate('ImportExtraFiles')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="importExtraFiles"
                          helpText={translate('ImportExtraFilesHelpText')}
                          onChange={onInputChange}
                          {...settings.importExtraFiles}
                        />
                      </FormGroup>

                      {
                        settings.importExtraFiles.value ?
                          <FormGroup
                            advancedSettings={advancedSettings}
                            isAdvanced={true}
                          >
                            <FormLabel>{translate('ImportExtraFiles')}</FormLabel>

                            <FormInputGroup
                              type={inputTypes.TEXT}
                              name="extraFileExtensions"
                              helpTexts={[
                                translate('ExtraFileExtensionsHelpTexts1'),
                                translate('ExtraFileExtensionsHelpTexts2')
                              ]}
                              onChange={onInputChange}
                              {...settings.extraFileExtensions}
                            />
                          </FormGroup> : null
                      }
                    </FieldSet> : null
                }

                <FieldSet
                  legend={translate('FileManagement')}
                >
                  <FormGroup size={sizes.MEDIUM}>
                    <FormLabel>{translate('IgnoreDeletedMovies')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="autoUnmonitorPreviouslyDownloadedMovies"
                      helpText={translate('AutoUnmonitorPreviouslyDownloadedMoviesHelpText')}
                      onChange={onInputChange}
                      {...settings.autoUnmonitorPreviouslyDownloadedMovies}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('DownloadPropersAndRepacks')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="downloadPropersAndRepacks"
                      helpTexts={[
                        translate('DownloadPropersAndRepacksHelpText1'),
                        translate('DownloadPropersAndRepacksHelpText2')
                      ]}
                      helpTextWarning={
                        settings.downloadPropersAndRepacks.value === 'doNotPrefer' ?
                          translate('DownloadPropersAndRepacksHelpTextWarning') :
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
                    <FormLabel>{translate('AnalyseVideoFiles')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="enableMediaInfo"
                      helpText={translate('EnableMediaInfoHelpText')}
                      onChange={onInputChange}
                      {...settings.enableMediaInfo}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('RescanMovieFolderAfterRefresh')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="rescanAfterRefresh"
                      helpText={translate('RescanAfterRefreshHelpText')}
                      helpTextWarning={translate('RescanAfterRefreshHelpTextWarning')}
                      values={rescanAfterRefreshOptions}
                      onChange={onInputChange}
                      {...settings.rescanAfterRefresh}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('ChangeFileDate')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="fileDate"
                      helpText={translate('FileDateHelpText')}
                      values={fileDateOptions}
                      onChange={onInputChange}
                      {...settings.fileDate}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('RecyclingBin')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.PATH}
                      name="recycleBin"
                      helpText={translate('RecycleBinHelpText')}
                      onChange={onInputChange}
                      {...settings.recycleBin}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('RecyclingBinCleanup')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.NUMBER}
                      name="recycleBinCleanupDays"
                      helpText={translate('RecycleBinCleanupDaysHelpText')}
                      helpTextWarning={translate('RecycleBinCleanupDaysHelpTextWarning')}
                      min={0}
                      onChange={onInputChange}
                      {...settings.recycleBinCleanupDays}
                    />
                  </FormGroup>
                </FieldSet>

                {
                  advancedSettings && !isWindows ?
                    <FieldSet
                      legend={translate('Permissions')}
                    >
                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>{translate('SetPermissions')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="setPermissionsLinux"
                          helpText={translate('SetPermissionsLinuxHelpText')}
                          helpTextWarning={translate('SetPermissionsLinuxHelpTextWarning')}
                          onChange={onInputChange}
                          {...settings.setPermissionsLinux}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>{translate('ChmodFolder')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.UMASK}
                          name="chmodFolder"
                          helpText={translate('ChmodFolderHelpText')}
                          helpTextWarning={translate('ChmodFolderHelpTextWarning')}
                          onChange={onInputChange}
                          {...settings.chmodFolder}
                        />
                      </FormGroup>

                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>{translate('ChmodGroup')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT}
                          name="chownGroup"
                          helpText={translate('ChmodGroupHelpText')}
                          helpTextWarning={translate('ChmodGroupHelpTextWarning')}
                          values={fileDateOptions}
                          onChange={onInputChange}
                          {...settings.chownGroup}
                        />
                      </FormGroup>
                    </FieldSet> : null
                }
              </Form> : null
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
