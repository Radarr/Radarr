import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withCurrentPage from 'Components/withCurrentPage';
import { clearBookFiles, fetchBookFiles } from 'Store/Actions/bookFileActions';
import { executeCommand } from 'Store/Actions/commandActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import * as wantedActions from 'Store/Actions/wantedActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import CutoffUnmet from './CutoffUnmet';

function createMapStateToProps() {
  return createSelector(
    (state) => state.wanted.cutoffUnmet,
    (state) => state.authors,
    createCommandExecutingSelector(commandNames.CUTOFF_UNMET_BOOK_SEARCH),
    (cutoffUnmet, authors, isSearchingForCutoffUnmetBooks) => {

      return {
        isAuthorFetching: authors.isFetching,
        isAuthorPopulated: authors.isPopulated,
        isSearchingForCutoffUnmetBooks,
        isSaving: cutoffUnmet.items.filter((m) => m.isSaving).length > 1,
        ...cutoffUnmet
      };
    }
  );
}

const mapDispatchToProps = {
  ...wantedActions,
  executeCommand,
  fetchQueueDetails,
  clearQueueDetails,
  fetchBookFiles,
  clearBookFiles
};

class CutoffUnmetConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchCutoffUnmet,
      gotoCutoffUnmetFirstPage
    } = this.props;

    registerPagePopulator(this.repopulate, ['bookFileUpdated']);

    if (useCurrentPage) {
      fetchCutoffUnmet();
    } else {
      gotoCutoffUnmetFirstPage();
    }
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      const bookIds = selectUniqueIds(this.props.items, 'id');
      const bookFileIds = selectUniqueIds(this.props.items, 'bookFileId');

      this.props.fetchQueueDetails({ bookIds });

      if (bookFileIds.length) {
        this.props.fetchBookFiles({ bookFileIds });
      }
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
    this.props.clearCutoffUnmet();
    this.props.clearQueueDetails();
    this.props.clearBookFiles();
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchCutoffUnmet();
  }

  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoCutoffUnmetFirstPage();
  }

  onPreviousPagePress = () => {
    this.props.gotoCutoffUnmetPreviousPage();
  }

  onNextPagePress = () => {
    this.props.gotoCutoffUnmetNextPage();
  }

  onLastPagePress = () => {
    this.props.gotoCutoffUnmetLastPage();
  }

  onPageSelect = (page) => {
    this.props.gotoCutoffUnmetPage({ page });
  }

  onSortPress = (sortKey) => {
    this.props.setCutoffUnmetSort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.setCutoffUnmetFilter({ selectedFilterKey });
  }

  onTableOptionChange = (payload) => {
    this.props.setCutoffUnmetTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoCutoffUnmetFirstPage();
    }
  }

  onSearchSelectedPress = (selected) => {
    this.props.executeCommand({
      name: commandNames.BOOK_SEARCH,
      bookIds: selected
    });
  }

  onSearchAllCutoffUnmetPress = () => {
    this.props.executeCommand({
      name: commandNames.CUTOFF_UNMET_BOOK_SEARCH
    });
  }

  //
  // Render

  render() {
    return (
      <CutoffUnmet
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onTableOptionChange={this.onTableOptionChange}
        onSearchSelectedPress={this.onSearchSelectedPress}
        onToggleSelectedPress={this.onToggleSelectedPress}
        onSearchAllCutoffUnmetPress={this.onSearchAllCutoffUnmetPress}
        {...this.props}
      />
    );
  }
}

CutoffUnmetConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchCutoffUnmet: PropTypes.func.isRequired,
  gotoCutoffUnmetFirstPage: PropTypes.func.isRequired,
  gotoCutoffUnmetPreviousPage: PropTypes.func.isRequired,
  gotoCutoffUnmetNextPage: PropTypes.func.isRequired,
  gotoCutoffUnmetLastPage: PropTypes.func.isRequired,
  gotoCutoffUnmetPage: PropTypes.func.isRequired,
  setCutoffUnmetSort: PropTypes.func.isRequired,
  setCutoffUnmetFilter: PropTypes.func.isRequired,
  setCutoffUnmetTableOption: PropTypes.func.isRequired,
  clearCutoffUnmet: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  fetchBookFiles: PropTypes.func.isRequired,
  clearBookFiles: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, mapDispatchToProps)(CutoffUnmetConnector)
);
