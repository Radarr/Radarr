import { push } from 'connected-react-router';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteBook } from 'Store/Actions/bookActions';
import createBookSelector from 'Store/Selectors/createBookSelector';
import DeleteBookModalContent from './DeleteBookModalContent';

function createMapStateToProps() {
  return createSelector(
    createBookSelector(),
    (book) => {
      return book;
    }
  );
}

const mapDispatchToProps = {
  push,
  deleteBook
};

class DeleteBookModalContentConnector extends Component {

  //
  // Listeners

  onDeletePress = (deleteFiles, addImportListExclusion) => {
    this.props.deleteBook({
      id: this.props.bookId,
      deleteFiles,
      addImportListExclusion
    });

    this.props.onModalClose(true);

    this.props.push(`${window.Readarr.urlBase}/author/${this.props.authorSlug}`);
  }

  //
  // Render

  render() {
    return (
      <DeleteBookModalContent
        {...this.props}
        onDeletePress={this.onDeletePress}
      />
    );
  }
}

DeleteBookModalContentConnector.propTypes = {
  bookId: PropTypes.number.isRequired,
  authorSlug: PropTypes.string.isRequired,
  push: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  deleteBook: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DeleteBookModalContentConnector);
