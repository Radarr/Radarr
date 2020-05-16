/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import createAllAuthorSelector from 'Store/Selectors/createAllAuthorsSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { clearBooks, fetchBooks } from 'Store/Actions/bookActions';
import { clearSeries, fetchSeries } from 'Store/Actions/seriesActions';
import { clearBookFiles, fetchBookFiles } from 'Store/Actions/bookFileActions';
import { toggleAuthorMonitored } from 'Store/Actions/authorActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import AuthorDetails from './AuthorDetails';

const selectBooks = createSelector(
  (state) => state.books,
  (books) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = books;

    const hasBooks = !!items.length;
    const hasMonitoredBooks = items.some((e) => e.monitored);

    return {
      isBooksFetching: isFetching,
      isBooksPopulated: isPopulated,
      booksError: error,
      hasBooks,
      hasMonitoredBooks
    };
  }
);

const selectSeries = createSelector(
  createSortedSectionSelector('series', (a, b) => a.title.localeCompare(b.title)),
  (state) => state.series,
  (series) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = series;

    const hasSeries = !!items.length;

    return {
      isSeriesFetching: isFetching,
      isSeriesPopulated: isPopulated,
      seriesError: error,
      hasSeries,
      series: series.items
    };
  }
);

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
    selectBooks,
    selectSeries,
    selectBookFiles,
    createAllAuthorSelector(),
    createCommandsSelector(),
    (titleSlug, books, series, bookFiles, allAuthors, commands) => {
      const sortedAuthor = _.orderBy(allAuthors, 'sortName');
      const authorIndex = _.findIndex(sortedAuthor, { titleSlug });
      const author = sortedAuthor[authorIndex];

      if (!author) {
        return {};
      }

      const {
        isBooksFetching,
        isBooksPopulated,
        booksError,
        hasBooks,
        hasMonitoredBooks
      } = books;

      const {
        isSeriesFetching,
        isSeriesPopulated,
        seriesError,
        hasSeries,
        series: seriesItems
      } = series;

      const {
        isBookFilesFetching,
        isBookFilesPopulated,
        bookFilesError,
        hasBookFiles
      } = bookFiles;

      const previousAuthor = sortedAuthor[authorIndex - 1] || _.last(sortedAuthor);
      const nextAuthor = sortedAuthor[authorIndex + 1] || _.first(sortedAuthor);
      const isAuthorRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_AUTHOR, authorId: author.id }));
      const authorRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_AUTHOR });
      const allAuthorRefreshing = (
        isCommandExecuting(authorRefreshingCommand) &&
        !authorRefreshingCommand.body.authorId
      );
      const isRefreshing = isAuthorRefreshing || allAuthorRefreshing;
      const isSearching = isCommandExecuting(findCommand(commands, { name: commandNames.AUTHOR_SEARCH, authorId: author.id }));
      const isRenamingFiles = isCommandExecuting(findCommand(commands, { name: commandNames.RENAME_FILES, authorId: author.id }));

      const isRenamingAuthorCommand = findCommand(commands, { name: commandNames.RENAME_AUTHOR });
      const isRenamingAuthor = (
        isCommandExecuting(isRenamingAuthorCommand) &&
        isRenamingAuthorCommand.body.authorIds.indexOf(author.id) > -1
      );

      const isFetching = isBooksFetching || isSeriesFetching || isBookFilesFetching;
      const isPopulated = isBooksPopulated && isSeriesPopulated && isBookFilesPopulated;

      const alternateTitles = _.reduce(author.alternateTitles, (acc, alternateTitle) => {
        if ((alternateTitle.seasonNumber === -1 || alternateTitle.seasonNumber === undefined) &&
            (alternateTitle.sceneSeasonNumber === -1 || alternateTitle.sceneSeasonNumber === undefined)) {
          acc.push(alternateTitle.title);
        }

        return acc;
      }, []);

      return {
        ...author,
        alternateTitles,
        isAuthorRefreshing,
        allAuthorRefreshing,
        isRefreshing,
        isSearching,
        isRenamingFiles,
        isRenamingAuthor,
        isFetching,
        isPopulated,
        booksError,
        seriesError,
        bookFilesError,
        hasBooks,
        hasMonitoredBooks,
        hasSeries,
        series: seriesItems,
        hasBookFiles,
        previousAuthor,
        nextAuthor
      };
    }
  );
}

const mapDispatchToProps = {
  fetchBooks,
  clearBooks,
  fetchSeries,
  clearSeries,
  fetchBookFiles,
  clearBookFiles,
  toggleAuthorMonitored,
  fetchQueueDetails,
  clearQueueDetails,
  clearReleases,
  cancelFetchReleases,
  executeCommand
};

class AuthorDetailsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      isAuthorRefreshing,
      allAuthorRefreshing,
      isRenamingFiles,
      isRenamingAuthor
    } = this.props;

    if (
      (prevProps.isAuthorRefreshing && !isAuthorRefreshing) ||
      (prevProps.allAuthorRefreshing && !allAuthorRefreshing) ||
      (prevProps.isRenamingFiles && !isRenamingFiles) ||
      (prevProps.isRenamingAuthor && !isRenamingAuthor)
    ) {
      this.populate();
    }

    // If the id has changed we need to clear the books
    // files and fetch from the server.

    if (prevProps.id !== id) {
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
    const authorId = this.props.id;

    this.props.fetchBooks({ authorId });
    this.props.fetchSeries({ authorId });
    this.props.fetchBookFiles({ authorId });
    this.props.fetchQueueDetails({ authorId });
  }

  unpopulate = () => {
    this.props.cancelFetchReleases();
    this.props.clearBooks();
    this.props.clearSeries();
    this.props.clearBookFiles();
    this.props.clearQueueDetails();
    this.props.clearReleases();
  }

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleAuthorMonitored({
      authorId: this.props.id,
      monitored
    });
  }

  onRefreshPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_AUTHOR,
      authorId: this.props.id
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.AUTHOR_SEARCH,
      authorId: this.props.id
    });
  }

  //
  // Render

  render() {
    return (
      <AuthorDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

AuthorDetailsConnector.propTypes = {
  id: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  isAuthorRefreshing: PropTypes.bool.isRequired,
  allAuthorRefreshing: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isRenamingFiles: PropTypes.bool.isRequired,
  isRenamingAuthor: PropTypes.bool.isRequired,
  fetchBooks: PropTypes.func.isRequired,
  clearBooks: PropTypes.func.isRequired,
  fetchSeries: PropTypes.func.isRequired,
  clearSeries: PropTypes.func.isRequired,
  fetchBookFiles: PropTypes.func.isRequired,
  clearBookFiles: PropTypes.func.isRequired,
  toggleAuthorMonitored: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  clearReleases: PropTypes.func.isRequired,
  cancelFetchReleases: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AuthorDetailsConnector);
