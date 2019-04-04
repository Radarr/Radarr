import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import {
  updateInteractiveImportItem,
  saveInteractiveImportItem
} from 'Store/Actions/interactiveImportActions';
import SelectAlbumReleaseModalContent from './SelectAlbumReleaseModalContent';

function createMapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  updateInteractiveImportItem,
  saveInteractiveImportItem
};

class SelectAlbumReleaseModalContentConnector extends Component {

  //
  // Listeners

  // onSortPress = (sortKey, sortDirection) => {
  //   this.props.setInteractiveImportAlbumsSort({ sortKey, sortDirection });
  // }

  onAlbumReleaseSelect = (albumId, albumReleaseId) => {
    const ids = this.props.importIdsByAlbum[albumId];

    ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        albumReleaseId,
        disableReleaseSwitching: true,
        tracks: [],
        rejections: []
      });
    });

    this.props.saveInteractiveImportItem({ id: ids });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectAlbumReleaseModalContent
        {...this.props}
        onAlbumReleaseSelect={this.onAlbumReleaseSelect}
      />
    );
  }
}

SelectAlbumReleaseModalContentConnector.propTypes = {
  importIdsByAlbum: PropTypes.object.isRequired,
  albums: PropTypes.arrayOf(PropTypes.object).isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  saveInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectAlbumReleaseModalContentConnector);
