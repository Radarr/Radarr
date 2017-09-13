import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import SelectAlbumModalContent from './SelectAlbumModalContent';

function createMapStateToProps() {
  return createSelector(
    createArtistSelector(),
    (series) => {
      return {
        items: series.albums
      };
    }
  );
}

const mapDispatchToProps = {
  updateInteractiveImportItem
};

class SelectAlbumModalContentConnector extends Component {

  //
  // Listeners

  onAlbumSelect = (albumId) => {
    const album = _.find(this.props.items, { id: albumId });

    this.props.ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        album,
        episodes: []
      });
    });

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
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectAlbumModalContentConnector);
