import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAuthorSelector from 'Store/Selectors/createAuthorSelector';
import { deleteAuthor } from 'Store/Actions/authorActions';
import DeleteAuthorModalContent from './DeleteAuthorModalContent';

function createMapStateToProps() {
  return createSelector(
    createAuthorSelector(),
    (author) => {
      return author;
    }
  );
}

const mapDispatchToProps = {
  deleteAuthor
};

class DeleteAuthorModalContentConnector extends Component {

  //
  // Listeners

  onDeletePress = (deleteFiles, addImportListExclusion) => {
    this.props.deleteAuthor({
      id: this.props.authorId,
      deleteFiles,
      addImportListExclusion
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <DeleteAuthorModalContent
        {...this.props}
        onDeletePress={this.onDeletePress}
      />
    );
  }
}

DeleteAuthorModalContentConnector.propTypes = {
  authorId: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired,
  deleteAuthor: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DeleteAuthorModalContentConnector);
