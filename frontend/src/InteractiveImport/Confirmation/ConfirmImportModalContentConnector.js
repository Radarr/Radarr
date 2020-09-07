import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearInteractiveImportBookFiles, fetchInteractiveImportBookFiles } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import ConfirmImportModalContent from './ConfirmImportModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport.bookFiles'),
    (bookFiles) => {
      return bookFiles;
    }
  );
}

const mapDispatchToProps = {
  fetchInteractiveImportBookFiles,
  clearInteractiveImportBookFiles
};

class ConfirmImportModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      books
    } = this.props;

    this.props.fetchInteractiveImportBookFiles({ bookId: books.map((x) => x.id) });
  }

  componentWillUnmount() {
    this.props.clearInteractiveImportBookFiles();
  }

  //
  // Render

  render() {
    return (
      <ConfirmImportModalContent
        {...this.props}
      />
    );
  }
}

ConfirmImportModalContentConnector.propTypes = {
  books: PropTypes.arrayOf(PropTypes.object).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchInteractiveImportBookFiles: PropTypes.func.isRequired,
  clearInteractiveImportBookFiles: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ConfirmImportModalContentConnector);
