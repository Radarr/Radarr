/* eslint max-params: 0 */
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
    (state) => state.settings.qualityProfiles,
    (state) => state.settings.metadataProfiles,
    (
      match,
      rootFolders,
      addArtist,
      importArtistState,
      qualityProfiles,
      metadataProfiles
    ) => {
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
        qualityProfiles: qualityProfiles.items,
        metadataProfiles: metadataProfiles.items,
        showMetadataProfile: metadataProfiles.items.length > 1,
        defaultQualityProfileId: addArtist.defaults.qualityProfileId,
        defaultMetadataProfileId: addArtist.defaults.metadataProfileId
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
  dispatchSetImportArtistValue: setImportArtistValue,
  dispatchImportArtist: importArtist,
  dispatchClearImportArtist: clearImportArtist,
  dispatchFetchRootFolders: fetchRootFolders,
  dispatchSetAddArtistDefault: setAddArtistDefault
};

class ImportArtistConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      qualityProfiles,
      metadataProfiles,
      defaultQualityProfileId,
      defaultMetadataProfileId,
      dispatchFetchRootFolders,
      dispatchSetAddArtistDefault
    } = this.props;

    if (!this.props.rootFoldersPopulated) {
      dispatchFetchRootFolders();
    }

    let setDefaults = false;
    const setDefaultPayload = {};

    if (
      !defaultQualityProfileId ||
      !qualityProfiles.some((p) => p.id === defaultQualityProfileId)
    ) {
      setDefaults = true;
      setDefaultPayload.qualityProfileId = qualityProfiles[0].id;
    }

    if (
      !defaultMetadataProfileId ||
      !metadataProfiles.some((p) => p.id === defaultMetadataProfileId)
    ) {
      setDefaults = true;
      setDefaultPayload.metadataProfileId = metadataProfiles[0].id;
    }

    if (setDefaults) {
      dispatchSetAddArtistDefault(setDefaultPayload);
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearImportArtist();
  }

  //
  // Listeners

  onInputChange = (ids, name, value) => {
    this.props.dispatchSetAddArtistDefault({ [name]: value });

    ids.forEach((id) => {
      this.props.dispatchSetImportArtistValue({
        id,
        [name]: value
      });
    });
  }

  onImportPress = (ids) => {
    this.props.dispatchImportArtist({ ids });
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
  qualityProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  metadataProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  defaultQualityProfileId: PropTypes.number.isRequired,
  defaultMetadataProfileId: PropTypes.number.isRequired,
  dispatchSetImportArtistValue: PropTypes.func.isRequired,
  dispatchImportArtist: PropTypes.func.isRequired,
  dispatchClearImportArtist: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchSetAddArtistDefault: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportArtistConnector);
