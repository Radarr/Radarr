import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteRootFolder } from 'Store/Actions/rootFolderActions';
import ImportArtistRootFolderRow from './ImportArtistRootFolderRow';

function createMapStateToProps() {
  return createSelector(
    () => {
      return {
      };
    }
  );
}

const mapDispatchToProps = {
  deleteRootFolder
};

class ImportArtistRootFolderRowConnector extends Component {

  //
  // Listeners

  onDeletePress = () => {
    this.props.deleteRootFolder({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <ImportArtistRootFolderRow
        {...this.props}
        onDeletePress={this.onDeletePress}
      />
    );
  }
}

ImportArtistRootFolderRowConnector.propTypes = {
  id: PropTypes.number.isRequired,
  deleteRootFolder: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportArtistRootFolderRowConnector);
