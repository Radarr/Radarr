import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import SelectInput from 'Components/Form/SelectInput';
import QualityProfileSelectInputConnector from 'Components/Form/QualityProfileSelectInputConnector';
import RootFolderSelectInputConnector from 'Components/Form/RootFolderSelectInputConnector';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import MoveSeriesModal from 'Movie/MoveSeries/MoveSeriesModal';
import TagsModal from './Tags/TagsModal';
import DeleteMovieModal from './Delete/DeleteMovieModal';
import SeriesEditorFooterLabel from './SeriesEditorFooterLabel';
import styles from './SeriesEditorFooter.css';

const NO_CHANGE = 'noChange';

class SeriesEditorFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      monitored: NO_CHANGE,
      qualityProfileId: NO_CHANGE,
      rootFolderPath: NO_CHANGE,
      savingTags: false,
      isDeleteMovieModalOpen: false,
      isTagsModalOpen: false,
      isConfirmMoveModalOpen: false,
      destinationRootFolder: null
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
      case 'rootFolderPath':
        this.setState({
          isConfirmMoveModalOpen: true,
          destinationRootFolder: value
        });
        break;
      case 'monitored':
        this.props.onSaveSelected({ [name]: value === 'monitored' });
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
    this.setState({ isDeleteMovieModalOpen: true });
  }

  onDeleteMovieModalClose = () => {
    this.setState({ isDeleteMovieModalOpen: false });
  }

  onTagsPress = () => {
    this.setState({ isTagsModalOpen: true });
  }

  onTagsModalClose = () => {
    this.setState({ isTagsModalOpen: false });
  }

  onSaveRootFolderPress = () => {
    this.setState({
      isConfirmMoveModalOpen: false,
      destinationRootFolder: null
    });

    this.props.onSaveSelected({ rootFolderPath: this.state.destinationRootFolder });
  }

  onMoveSeriesPress = () => {
    this.setState({
      isConfirmMoveModalOpen: false,
      destinationRootFolder: null
    });

    this.props.onSaveSelected({
      rootFolderPath: this.state.destinationRootFolder,
      moveFiles: true
    });
  }

  //
  // Render

  render() {
    const {
      seriesIds,
      selectedCount,
      isSaving,
      isDeleting,
      isOrganizingSeries,
      onOrganizeSeriesPress
    } = this.props;

    const {
      monitored,
      qualityProfileId,
      rootFolderPath,
      savingTags,
      isTagsModalOpen,
      isDeleteMovieModalOpen,
      isConfirmMoveModalOpen,
      destinationRootFolder
    } = this.state;

    const monitoredOptions = [
      { key: NO_CHANGE, value: 'No Change', disabled: true },
      { key: 'monitored', value: 'Monitored' },
      { key: 'unmonitored', value: 'Unmonitored' }
    ];

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <SeriesEditorFooterLabel
            label="Monitor Series"
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
          <SeriesEditorFooterLabel
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

        <div className={styles.inputContainer}>
          <SeriesEditorFooterLabel
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
            <SeriesEditorFooterLabel
              label={`${selectedCount} Series Selected`}
              isSaving={false}
            />

            <div className={styles.buttons}>
              <div>
                <SpinnerButton
                  className={styles.organizeSelectedButton}
                  kind={kinds.WARNING}
                  isSpinning={isOrganizingSeries}
                  isDisabled={!selectedCount || isOrganizingSeries}
                  onPress={onOrganizeSeriesPress}
                >
                  Rename Files
                </SpinnerButton>

                <SpinnerButton
                  className={styles.tagsButton}
                  isSpinning={isSaving && savingTags}
                  isDisabled={!selectedCount || isOrganizingSeries}
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
          seriesIds={seriesIds}
          onApplyTagsPress={this.onApplyTagsPress}
          onModalClose={this.onTagsModalClose}
        />

        <DeleteMovieModal
          isOpen={isDeleteMovieModalOpen}
          seriesIds={seriesIds}
          onModalClose={this.onDeleteMovieModalClose}
        />

        <MoveSeriesModal
          destinationRootFolder={destinationRootFolder}
          isOpen={isConfirmMoveModalOpen}
          onSavePress={this.onSaveRootFolderPress}
          onMoveSeriesPress={this.onMoveSeriesPress}
        />
      </PageContentFooter>
    );
  }
}

SeriesEditorFooter.propTypes = {
  seriesIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  selectedCount: PropTypes.number.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  isOrganizingSeries: PropTypes.bool.isRequired,
  onSaveSelected: PropTypes.func.isRequired,
  onOrganizeSeriesPress: PropTypes.func.isRequired
};

export default SeriesEditorFooter;
