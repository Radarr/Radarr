import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import SelectInput from 'Components/Form/SelectInput';
import LanguageProfileSelectInputConnector from 'Components/Form/LanguageProfileSelectInputConnector';
import MetadataProfileSelectInputConnector from 'Components/Form/MetadataProfileSelectInputConnector';
import QualityProfileSelectInputConnector from 'Components/Form/QualityProfileSelectInputConnector';
import RootFolderSelectInputConnector from 'Components/Form/RootFolderSelectInputConnector';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import TagsModal from './Tags/TagsModal';
import DeleteArtistModal from './Delete/DeleteArtistModal';
import ArtistEditorFooterLabel from './ArtistEditorFooterLabel';
import styles from './ArtistEditorFooter.css';

const NO_CHANGE = 'noChange';

class ArtistEditorFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      monitored: NO_CHANGE,
      qualityProfileId: NO_CHANGE,
      languageProfileId: NO_CHANGE,
      metadataProfileId: NO_CHANGE,
      albumFolder: NO_CHANGE,
      rootFolderPath: NO_CHANGE,
      savingTags: false,
      isDeleteArtistModalOpen: false,
      isTagsModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isSaving,
      saveError
    } = this.props;

    if (prevProps.isSaving && !isSaving && !saveError) {
      this.setState({
        monitored: NO_CHANGE,
        qualityProfileId: NO_CHANGE,
        languageProfileId: NO_CHANGE,
        metadataProfileId: NO_CHANGE,
        albumFolder: NO_CHANGE,
        rootFolderPath: NO_CHANGE,
        savingTags: false
      });
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });

    if (value === NO_CHANGE) {
      return;
    }

    switch (name) {
      case 'monitored':
        this.props.onSaveSelected({ [name]: value === 'monitored' });
        break;
      case 'albumFolder':
        this.props.onSaveSelected({ [name]: value === 'yes' });
        break;
      default:
        this.props.onSaveSelected({ [name]: value });
    }
  }

  onApplyTagsPress = (tags, applyTags) => {
    this.setState({
      savingTags: true,
      isTagsModalOpen: false
    });

    this.props.onSaveSelected({
      tags,
      applyTags
    });
  }

  onDeleteSelectedPress = () => {
    this.setState({ isDeleteArtistModalOpen: true });
  }

  onDeleteArtistModalClose = () => {
    this.setState({ isDeleteArtistModalOpen: false });
  }

  onTagsPress = () => {
    this.setState({ isTagsModalOpen: true });
  }

  onTagsModalClose = () => {
    this.setState({ isTagsModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      artistIds,
      selectedCount,
      isSaving,
      isDeleting,
      isOrganizingArtist,
      showLanguageProfile,
      showMetadataProfile,
      onOrganizeArtistPress
    } = this.props;

    const {
      monitored,
      qualityProfileId,
      languageProfileId,
      metadataProfileId,
      albumFolder,
      rootFolderPath,
      savingTags,
      isTagsModalOpen,
      isDeleteArtistModalOpen
    } = this.state;

    const monitoredOptions = [
      { key: NO_CHANGE, value: 'No Change', disabled: true },
      { key: 'monitored', value: 'Monitored' },
      { key: 'unmonitored', value: 'Unmonitored' }
    ];

    const albumFolderOptions = [
      { key: NO_CHANGE, value: 'No Change', disabled: true },
      { key: 'yes', value: 'Yes' },
      { key: 'no', value: 'No' }
    ];

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <ArtistEditorFooterLabel
            label="Monitor Artist"
            isSaving={isSaving && monitored !== NO_CHANGE}
          />

          <SelectInput
            name="monitored"
            value={monitored}
            values={monitoredOptions}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <ArtistEditorFooterLabel
            label="Quality Profile"
            isSaving={isSaving && qualityProfileId !== NO_CHANGE}
          />

          <QualityProfileSelectInputConnector
            name="qualityProfileId"
            value={qualityProfileId}
            includeNoChange={true}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        {
          showLanguageProfile &&
            <div className={styles.inputContainer}>
              <ArtistEditorFooterLabel
                label="Language Profile"
                isSaving={isSaving && languageProfileId !== NO_CHANGE}
              />

              <LanguageProfileSelectInputConnector
                name="languageProfileId"
                value={languageProfileId}
                includeNoChange={true}
                isDisabled={!selectedCount}
                onChange={this.onInputChange}
              />
            </div>
        }

        {
          showMetadataProfile &&
            <div className={styles.inputContainer}>
              <ArtistEditorFooterLabel
                label="Metadata Profile"
                isSaving={isSaving && metadataProfileId !== NO_CHANGE}
              />

              <MetadataProfileSelectInputConnector
                name="metadataProfileId"
                value={metadataProfileId}
                includeNoChange={true}
                isDisabled={!selectedCount}
                onChange={this.onInputChange}
              />
            </div>
        }

        <div className={styles.inputContainer}>
          <ArtistEditorFooterLabel
            label="Album Folder"
            isSaving={isSaving && albumFolder !== NO_CHANGE}
          />

          <SelectInput
            name="albumFolder"
            value={albumFolder}
            values={albumFolderOptions}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <ArtistEditorFooterLabel
            label="Root Folder"
            isSaving={isSaving && rootFolderPath !== NO_CHANGE}
          />

          <RootFolderSelectInputConnector
            name="rootFolderPath"
            value={rootFolderPath}
            includeNoChange={true}
            isDisabled={!selectedCount}
            selectedValueOptions={{ includeFreeSpace: false }}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.buttonContainer}>
          <div className={styles.buttonContainerContent}>
            <ArtistEditorFooterLabel
              label={`${selectedCount} Artist(s) Selected`}
              isSaving={false}
            />

            <div className={styles.buttons}>
              <div>
                <SpinnerButton
                  className={styles.organizeSelectedButton}
                  kind={kinds.WARNING}
                  isSpinning={isOrganizingArtist}
                  isDisabled={!selectedCount || isOrganizingArtist}
                  onPress={onOrganizeArtistPress}
                >
                  Rename Files
                </SpinnerButton>

                <SpinnerButton
                  className={styles.tagsButton}
                  isSpinning={isSaving && savingTags}
                  isDisabled={!selectedCount || isOrganizingArtist}
                  onPress={this.onTagsPress}
                >
                  Set Tags
                </SpinnerButton>
              </div>

              <SpinnerButton
                className={styles.deleteSelectedButton}
                kind={kinds.DANGER}
                isSpinning={isDeleting}
                isDisabled={!selectedCount || isDeleting}
                onPress={this.onDeleteSelectedPress}
              >
                Delete
              </SpinnerButton>
            </div>
          </div>
        </div>

        <TagsModal
          isOpen={isTagsModalOpen}
          artistIds={artistIds}
          onApplyTagsPress={this.onApplyTagsPress}
          onModalClose={this.onTagsModalClose}
        />

        <DeleteArtistModal
          isOpen={isDeleteArtistModalOpen}
          artistIds={artistIds}
          onModalClose={this.onDeleteArtistModalClose}
        />
      </PageContentFooter>
    );
  }
}

ArtistEditorFooter.propTypes = {
  artistIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  selectedCount: PropTypes.number.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  isOrganizingArtist: PropTypes.bool.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  showMetadataProfile: PropTypes.bool.isRequired,
  onSaveSelected: PropTypes.func.isRequired,
  onOrganizeArtistPress: PropTypes.func.isRequired
};

export default ArtistEditorFooter;
