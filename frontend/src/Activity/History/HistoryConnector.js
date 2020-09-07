import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import withCurrentPage from 'Components/withCurrentPage';
import { clearBooks, fetchBooks } from 'Store/Actions/bookActions';
import * as historyActions from 'Store/Actions/historyActions';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import History from './History';

function createMapStateToProps() {
  return createSelector(
    (state) => state.history,
    (state) => state.authors,
    (state) => state.books,
    (history, authors, books) => {
      return {
        isAuthorFetching: authors.isFetching,
        isAuthorPopulated: authors.isPopulated,
        isBooksFetching: books.isFetching,
        isBooksPopulated: books.isPopulated,
        booksError: books.error,
        ...history
      };
    }
  );
}

const mapDispatchToProps = {
  ...historyActions,
  fetchBooks,
  clearBooks
};

class HistoryConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchHistory,
      gotoHistoryFirstPage
    } = this.props;

    registerPagePopulator(this.repopulate);

    if (useCurrentPage) {
      fetchHistory();
    } else {
      gotoHistoryFirstPage();
    }
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      const bookIds = selectUniqueIds(this.props.items, 'bookId');
      if (bookIds.length) {
        this.props.fetchBooks({ bookIds });
      } else {
        this.props.clearBooks();
      }
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
    this.props.clearHistory();
    this.props.clearBooks();
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchHistory();
  }

  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoHistoryFirstPage();
  }

  onPreviousPagePress = () => {
    this.props.gotoHistoryPreviousPage();
  }

  onNextPagePress = () => {
    this.props.gotoHistoryNextPage();
  }

  onLastPagePress = () => {
    this.props.gotoHistoryLastPage();
  }

  onPageSelect = (page) => {
    this.props.gotoHistoryPage({ page });
  }

  onSortPress = (sortKey) => {
    this.props.setHistorySort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.setHistoryFilter({ selectedFilterKey });
  }

  onTableOptionChange = (payload) => {
    this.props.setHistoryTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoHistoryFirstPage();
    }
  }

  //
  // Render

  render() {
    return (
      <History
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onTableOptionChange={this.onTableOptionChange}
        {...this.props}
      />
    );
  }
}

HistoryConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchHistory: PropTypes.func.isRequired,
  gotoHistoryFirstPage: PropTypes.func.isRequired,
  gotoHistoryPreviousPage: PropTypes.func.isRequired,
  gotoHistoryNextPage: PropTypes.func.isRequired,
  gotoHistoryLastPage: PropTypes.func.isRequired,
  gotoHistoryPage: PropTypes.func.isRequired,
  setHistorySort: PropTypes.func.isRequired,
  setHistoryFilter: PropTypes.func.isRequired,
  setHistoryTableOption: PropTypes.func.isRequired,
  clearHistory: PropTypes.func.isRequired,
  fetchBooks: PropTypes.func.isRequired,
  clearBooks: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, mapDispatchToProps)(HistoryConnector)
);
