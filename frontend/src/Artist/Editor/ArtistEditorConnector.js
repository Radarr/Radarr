import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandSelector from 'Store/Selectors/createCommandSelector';
import { setArtistEditorSort, setArtistEditorFilter, saveArtistEditor } from 'Store/Actions/artistEditorActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
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
  setArtistEditorSort,
  setArtistEditorFilter,
  saveArtistEditor,
  fetchRootFolders
};

class ArtistEditorConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchRootFolders();
  }

  //
  // Listeners

  onSortPress = (sortKey) => {
    this.props.setArtistEditorSort({ sortKey });
  }

  onFilterSelect = (filterKey, filterValue, filterType) => {
    this.props.setArtistEditorFilter({ filterKey, filterValue, filterType });
  }

  onSaveSelected = (payload) => {
    this.props.saveArtistEditor(payload);
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
  setArtistEditorSort: PropTypes.func.isRequired,
  setArtistEditorFilter: PropTypes.func.isRequired,
  saveArtistEditor: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'artist', uiSection: 'artistEditor' }
)(ArtistEditorConnector);
