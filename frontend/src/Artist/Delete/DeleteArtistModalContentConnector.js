import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { deleteArtist } from 'Store/Actions/artistActions';
import DeleteArtistModalContent from './DeleteArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    createArtistSelector(),
    (artist) => {
      return artist;
    }
  );
}

const mapDispatchToProps = {
  deleteArtist
};

class DeleteArtistModalContentConnector extends Component {

  //
  // Listeners

  onDeletePress = (deleteFiles, addImportListExclusion) => {
    this.props.deleteArtist({
      id: this.props.artistId,
      deleteFiles,
      addImportListExclusion
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <DeleteArtistModalContent
        {...this.props}
        onDeletePress={this.onDeletePress}
      />
    );
  }
}

DeleteArtistModalContentConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired,
  deleteArtist: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DeleteArtistModalContentConnector);
