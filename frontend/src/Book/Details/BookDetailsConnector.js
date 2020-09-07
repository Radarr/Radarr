/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { toggleBooksMonitored } from 'Store/Actions/bookActions';
import { clearBookFiles, fetchBookFiles } from 'Store/Actions/bookFileActions';
import { executeCommand } from 'Store/Actions/commandActions';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import createAllAuthorSelector from 'Store/Selectors/createAllAuthorsSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import BookDetails from './BookDetails';

const selectBookFiles = createSelector(
  (state) => state.bookFiles,
  (bookFiles) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = bookFiles;

    const hasBookFiles = !!items.length;

    return {
      isBookFilesFetching: isFetching,
      isBookFilesPopulated: isPopulated,
      bookFilesError: error,
      hasBookFiles
    };
  }
);

function createMapStateToProps() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    selectBookFiles,
    (state) => state.books,
    createAllAuthorSelector(),
    createCommandsSelector(),
    createUISettingsSelector(),
    (titleSlug, bookFiles, books, authors, commands, uiSettings) => {
      const sortedBooks = _.orderBy(books.items, 'releaseDate');
      const bookIndex = _.findIndex(sortedBooks, { titleSlug });
      const book = sortedBooks[bookIndex];
      const author = _.find(authors, { id: book.authorId });

      if (!book) {
        return {};
      }

      const {
        isBookFilesFetching,
        isBookFilesPopulated,
        bookFilesError,
        hasBookFiles
      } = bookFiles;

      const previousBook = sortedBooks[bookIndex - 1] || _.last(sortedBooks);
      const nextBook = sortedBooks[bookIndex + 1] || _.first(sortedBooks);
      const isSearchingCommand = findCommand(commands, { name: commandNames.BOOK_SEARCH });
      const isSearching = (
        isCommandExecuting(isSearchingCommand) &&
        isSearchingCommand.body.bookIds.indexOf(book.id) > -1
      );

      const isRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_BOOK });
      const isRefreshing = (
        isCommandExecuting(isRefreshingCommand) &&
        isRefreshingCommand.body.bookId === book.id
      );

      const isFetching = isBookFilesFetching;
      const isPopulated = isBookFilesPopulated;

      return {
        ...book,
        shortDateFormat: uiSettings.shortDateFormat,
        author,
        isRefreshing,
        isSearching,
        isFetching,
        isPopulated,
        bookFilesError,
        hasBookFiles,
        previousBook,
        nextBook
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand,
  fetchBookFiles,
  clearBookFiles,
  clearReleases,
  cancelFetchReleases,
  toggleBooksMonitored
};

function getMonitoredEditions(props) {
  return _.map(_.filter(props.editions, { monitored: true }), 'id').sort();
}

class BookDetailsConnector extends Component {

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentDidUpdate(prevProps) {
    if (!_.isEqual(getMonitoredEditions(prevProps), getMonitoredEditions(this.props)) ||
        (prevProps.anyReleaseOk === false && this.props.anyReleaseOk === true)) {
      this.unpopulate();
      this.populate();
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.populate);
    this.unpopulate();
  }

  //
  // Control

  populate = () => {
    const bookId = this.props.id;

    this.props.fetchBookFiles({ bookId });
  }

  unpopulate = () => {
    this.props.cancelFetchReleases();
    this.props.clearReleases();
    this.props.clearBookFiles();
  }

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleBooksMonitored({
      bookIds: [this.props.id],
      monitored
    });
  }

  onRefreshPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_BOOK,
      bookId: this.props.id
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.BOOK_SEARCH,
      bookIds: [this.props.id]
    });
  }

  //
  // Render

  render() {
    return (
      <BookDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

BookDetailsConnector.propTypes = {
  id: PropTypes.number,
  anyReleaseOk: PropTypes.bool,
  isBookFetching: PropTypes.bool,
  isBookPopulated: PropTypes.bool,
  titleSlug: PropTypes.string.isRequired,
  fetchBookFiles: PropTypes.func.isRequired,
  clearBookFiles: PropTypes.func.isRequired,
  clearReleases: PropTypes.func.isRequired,
  cancelFetchReleases: PropTypes.func.isRequired,
  toggleBooksMonitored: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(BookDetailsConnector);
