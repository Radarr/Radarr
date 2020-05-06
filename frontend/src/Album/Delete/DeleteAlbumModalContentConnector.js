import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { push } from 'connected-react-router';
import createAlbumSelector from 'Store/Selectors/createAlbumSelector';
import { deleteAlbum } from 'Store/Actions/albumActions';
import DeleteAlbumModalContent from './DeleteAlbumModalContent';

function createMapStateToProps() {
  return createSelector(
    createAlbumSelector(),
    (album) => {
      return album;
    }
  );
}

const mapDispatchToProps = {
  push,
  deleteAlbum
};

class DeleteAlbumModalContentConnector extends Component {

  //
  // Listeners

  onDeletePress = (deleteFiles, addImportListExclusion) => {
    this.props.deleteAlbum({
      id: this.props.bookId,
      deleteFiles,
      addImportListExclusion
    });

    this.props.onModalClose(true);

    this.props.push(`${window.Readarr.urlBase}/author/${this.props.titleSlug}`);
  }

  //
  // Render

  render() {
    return (
      <DeleteAlbumModalContent
        {...this.props}
        onDeletePress={this.onDeletePress}
      />
    );
  }
}

DeleteAlbumModalContentConnector.propTypes = {
  bookId: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  push: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  deleteAlbum: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DeleteAlbumModalContentConnector);
