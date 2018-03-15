import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandSelector from 'Store/Selectors/createCommandSelector';
import { setArtistEditorSort, setArtistEditorFilter, saveArtistEditor } from 'Store/Actions/artistEditorActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import ArtistEditor from './ArtistEditor';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.languageProfiles,
    (state) => state.settings.metadataProfiles,
    createClientSideCollectionSelector(),
    createCommandSelector(commandNames.RENAME_ARTIST),
    (languageProfiles, metadataProfiles, artist, isOrganizingArtist) => {
      return {
        isOrganizingArtist,
        showLanguageProfile: languageProfiles.items.length > 1,
        showMetadataProfile: metadataProfiles.items.length > 1,
        ...artist
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetArtistEditorSort: setArtistEditorSort,
  dispatchSetArtistEditorFilter: setArtistEditorFilter,
  dispatchSaveArtistEditor: saveArtistEditor,
  dispatchFetchRootFolders: fetchRootFolders,
  dispatchExecuteCommand: executeCommand
};

class ArtistEditorConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchRootFolders();
  }

  //
  // Listeners

  onSortPress = (sortKey) => {
    this.props.dispatchSetArtistEditorSort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.dispatchSetArtistEditorFilter({ selectedFilterKey });
  }

  onSaveSelected = (payload) => {
    this.props.dispatchSaveArtistEditor(payload);
  }

  onMoveSelected = (payload) => {
    this.props.dispatchExecuteCommand({
      name: commandNames.MOVE_ARTIST,
      ...payload
    });
  }

  //
  // Render

  render() {
    return (
      <ArtistEditor
        {...this.props}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onSaveSelected={this.onSaveSelected}
      />
    );
  }
}

ArtistEditorConnector.propTypes = {
  dispatchSetArtistEditorSort: PropTypes.func.isRequired,
  dispatchSetArtistEditorFilter: PropTypes.func.isRequired,
  dispatchSaveArtistEditor: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'artist', uiSection: 'artistEditor' }
)(ArtistEditorConnector);
