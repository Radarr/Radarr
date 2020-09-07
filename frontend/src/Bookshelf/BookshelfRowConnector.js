import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { toggleAuthorMonitored } from 'Store/Actions/authorActions';
import { toggleBooksMonitored } from 'Store/Actions/bookActions';
import createAuthorSelector from 'Store/Selectors/createAuthorSelector';
import BookshelfRow from './BookshelfRow';

// Use a const to share the reselect cache between instances
const getBookMap = createSelector(
  (state) => state.books.items,
  (books) => {
    return books.reduce((acc, curr) => {
      (acc[curr.authorId] = acc[curr.authorId] || []).push(curr);
      return acc;
    }, {});
  }
);

function createMapStateToProps() {
  return createSelector(
    createAuthorSelector(),
    getBookMap,
    (author, bookMap) => {
      const booksInAuthor = bookMap.hasOwnProperty(author.id) ? bookMap[author.id] : [];
      const sortedBooks = _.orderBy(booksInAuthor, 'releaseDate', 'desc');

      return {
        ...author,
        authorId: author.id,
        authorName: author.authorName,
        monitored: author.monitored,
        status: author.status,
        isSaving: author.isSaving,
        books: sortedBooks
      };
    }
  );
}

const mapDispatchToProps = {
  toggleAuthorMonitored,
  toggleBooksMonitored
};

class BookshelfRowConnector extends Component {

  //
  // Listeners

  onAuthorMonitoredPress = () => {
    const {
      authorId,
      monitored
    } = this.props;

    this.props.toggleAuthorMonitored({
      authorId,
      monitored: !monitored
    });
  }

  onBookMonitoredPress = (bookId, monitored) => {
    const bookIds = [bookId];
    this.props.toggleBooksMonitored({
      bookIds,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <BookshelfRow
        {...this.props}
        onAuthorMonitoredPress={this.onAuthorMonitoredPress}
        onBookMonitoredPress={this.onBookMonitoredPress}
      />
    );
  }
}

BookshelfRowConnector.propTypes = {
  authorId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  toggleAuthorMonitored: PropTypes.func.isRequired,
  toggleBooksMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(BookshelfRowConnector);
