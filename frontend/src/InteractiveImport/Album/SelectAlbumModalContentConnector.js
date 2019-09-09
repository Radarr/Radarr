import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import {
  updateInteractiveImportItem,
  saveInteractiveImportItem,
  fetchInteractiveImportAlbums,
  setInteractiveImportAlbumsSort,
  clearInteractiveImportAlbums
} from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import SelectAlbumModalContent from './SelectAlbumModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport.albums'),
    (albums) => {
      return albums;
    }
  );
}

const mapDispatchToProps = {
  fetchInteractiveImportAlbums,
  setInteractiveImportAlbumsSort,
  clearInteractiveImportAlbums,
  updateInteractiveImportItem,
  saveInteractiveImportItem
};

class SelectAlbumModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      artistId
    } = this.props;

    this.props.fetchInteractiveImportAlbums({ artistId });
  }

  componentWillUnmount() {
    // This clears the albums for the queue and hides the queue
    // We'll need another place to store albums for manual import
    this.props.clearInteractiveImportAlbums();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.setInteractiveImportAlbumsSort({ sortKey, sortDirection });
  }

  onAlbumSelect = (albumId) => {
    const album = _.find(this.props.items, { id: albumId });

    const ids = this.props.ids;

    ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        album,
        albumReleaseId: undefined,
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
      <SelectAlbumModalContent
        {...this.props}
        onAlbumSelect={this.onAlbumSelect}
      />
    );
  }
}

SelectAlbumModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  artistId: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchInteractiveImportAlbums: PropTypes.func.isRequired,
  setInteractiveImportAlbumsSort: PropTypes.func.isRequired,
  clearInteractiveImportAlbums: PropTypes.func.isRequired,
  saveInteractiveImportItem: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectAlbumModalContentConnector);
