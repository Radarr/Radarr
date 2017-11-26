import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setImportArtistValue, importArtist, clearImportArtist } from 'Store/Actions/importArtistActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { setAddArtistDefault } from 'Store/Actions/addArtistActions';
import createRouteMatchShape from 'Helpers/Props/Shapes/createRouteMatchShape';
import ImportArtist from './ImportArtist';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.rootFolders,
    (state) => state.addArtist,
    (state) => state.importArtist,
    (state) => state.settings.languageProfiles,
    (state) => state.settings.metadataProfiles,
    (match, rootFolders, addArtist, importArtistState, languageProfiles, metadataProfiles) => {
      const {
        isFetching: rootFoldersFetching,
        isPopulated: rootFoldersPopulated,
        error: rootFoldersError,
        items
      } = rootFolders;

      const rootFolderId = parseInt(match.params.rootFolderId);

      const result = {
        rootFolderId,
        rootFoldersFetching,
        rootFoldersPopulated,
        rootFoldersError,
        showLanguageProfile: languageProfiles.items.length > 1,
        showMetadataProfile: metadataProfiles.items.length > 1
      };

      if (items.length) {
        const rootFolder = _.find(items, { id: rootFolderId });

        return {
          ...result,
          ...rootFolder,
          items: importArtistState.items
        };
      }

      return result;
    }
  );
}

const mapDispatchToProps = {
  setImportArtistValue,
  importArtist,
  clearImportArtist,
  fetchRootFolders,
  setAddArtistDefault
};

class ImportArtistConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.rootFoldersPopulated) {
      this.props.fetchRootFolders();
    }
  }

  componentWillUnmount() {
    this.props.clearImportArtist();
  }

  //
  // Listeners

  onInputChange = (ids, name, value) => {
    this.props.setAddArtistDefault({ [name]: value });

    ids.forEach((id) => {
      this.props.setImportArtistValue({
        id,
        [name]: value
      });
    });
  }

  onImportPress = (ids) => {
    this.props.importArtist({ ids });
  }

  //
  // Render

  render() {
    return (
      <ImportArtist
        {...this.props}
        onInputChange={this.onInputChange}
        onImportPress={this.onImportPress}
      />
    );
  }
}

const routeMatchShape = createRouteMatchShape({
  rootFolderId: PropTypes.string.isRequired
});

ImportArtistConnector.propTypes = {
  match: routeMatchShape.isRequired,
  rootFoldersPopulated: PropTypes.bool.isRequired,
  setImportArtistValue: PropTypes.func.isRequired,
  importArtist: PropTypes.func.isRequired,
  clearImportArtist: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired,
  setAddArtistDefault: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportArtistConnector);
