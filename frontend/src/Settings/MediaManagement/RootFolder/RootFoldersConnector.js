import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteRootFolder, fetchRootFolders } from 'Store/Actions/settingsActions';
import RootFolders from './RootFolders';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.rootFolders,
    (state) => state.settings.qualityProfiles,
    (state) => state.settings.metadataProfiles,
    (rootFolders, quality, metadata) => {
      return {
        qualityProfiles: quality.items,
        metadataProfiles: metadata.items,
        ...rootFolders
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchRootFolders: fetchRootFolders,
  dispatchDeleteRootFolder: deleteRootFolder
};

class RootFoldersConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchRootFolders();
  }

  //
  // Listeners

  onConfirmDeleteRootFolder = (id) => {
    this.props.dispatchDeleteRootFolder({ id });
  }

  //
  // Render

  render() {
    return (
      <RootFolders
        {...this.props}
        onConfirmDeleteRootFolder={this.onConfirmDeleteRootFolder}
      />
    );
  }
}

RootFoldersConnector.propTypes = {
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchDeleteRootFolder: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(RootFoldersConnector);
