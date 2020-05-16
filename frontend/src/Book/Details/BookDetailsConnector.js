/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { toggleBooksMonitored } from 'Store/Actions/bookActions';
import { fetchBookFiles, clearBookFiles } from 'Store/Actions/bookFileActions';
import { clearReleases, cancelFetchReleases } from 'Store/Actions/releaseActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import BookDetails from './BookDetails';
import createAllAuthorSelector from 'Store/Selectors/createAllAuthorsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';

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

      const isFetching = isBookFilesFetching;
      const isPopulated = isBookFilesPopulated;

      return {
        ...book,
        shortDateFormat: uiSettings.shortDateFormat,
        author,
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

class BookDetailsConnector extends Component {

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentDidUpdate(prevProps) {
    // If the id has changed we need to clear the books
    // files and fetch from the server.

    if (prevProps.id !== this.props.id) {
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
