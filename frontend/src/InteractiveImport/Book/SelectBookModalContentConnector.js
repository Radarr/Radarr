import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import {
  updateInteractiveImportItem,
  saveInteractiveImportItem,
  fetchInteractiveImportBooks,
  setInteractiveImportBooksSort,
  clearInteractiveImportBooks
} from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import SelectBookModalContent from './SelectBookModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport.books'),
    (books) => {
      return books;
    }
  );
}

const mapDispatchToProps = {
  fetchInteractiveImportBooks,
  setInteractiveImportBooksSort,
  clearInteractiveImportBooks,
  updateInteractiveImportItem,
  saveInteractiveImportItem
};

class SelectBookModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      authorId
    } = this.props;

    this.props.fetchInteractiveImportBooks({ authorId });
  }

  componentWillUnmount() {
    // This clears the books for the queue and hides the queue
    // We'll need another place to store books for manual import
    this.props.clearInteractiveImportBooks();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.setInteractiveImportBooksSort({ sortKey, sortDirection });
  }

  onBookSelect = (bookId) => {
    const book = _.find(this.props.items, { id: bookId });

    const ids = this.props.ids;

    ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        book,
        bookReleaseId: undefined,
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
      <SelectBookModalContent
        {...this.props}
        onSortPress={this.onSortPress}
        onBookSelect={this.onBookSelect}
      />
    );
  }
}

SelectBookModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  authorId: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchInteractiveImportBooks: PropTypes.func.isRequired,
  setInteractiveImportBooksSort: PropTypes.func.isRequired,
  clearInteractiveImportBooks: PropTypes.func.isRequired,
  saveInteractiveImportItem: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectBookModalContentConnector);
