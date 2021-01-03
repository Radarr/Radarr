import { push } from 'connected-react-router';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import NotFound from 'Components/NotFound';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { clearBooks, fetchBooks } from 'Store/Actions/bookActions';
import BookDetailsConnector from './BookDetailsConnector';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.books,
    (state) => state.authors,
    (match, books, author) => {
      const titleSlug = match.params.titleSlug;
      const isFetching = books.isFetching || author.isFetching;
      const isPopulated = books.isPopulated && author.isPopulated;

      // if books have been fetched, make sure requested one exists
      // otherwise don't map titleSlug to trigger not found page
      if (!isFetching && isPopulated) {
        const bookIndex = _.findIndex(books.items, { titleSlug });
        if (bookIndex === -1) {
          return {
            isFetching,
            isPopulated
          };
        }
      }

      return {
        titleSlug,
        isFetching,
        isPopulated
      };
    }
  );
}

const mapDispatchToProps = {
  push,
  fetchBooks,
  clearBooks
};

class BookDetailsPageConnector extends Component {

  constructor(props) {
    super(props);
    this.state = { hasMounted: false };
  }
  //
  // Lifecycle

  componentDidMount() {
    this.populate();
  }

  componentWillUnmount() {
    this.unpopulate();
  }

  //
  // Control

  populate = () => {
    const titleSlug = this.props.titleSlug;
    this.setState({ hasMounted: true });
    this.props.fetchBooks({
      titleSlug,
      includeAllAuthorBooks: true
    });
  }

  unpopulate = () => {
    this.props.clearBooks();
  }

  //
  // Render

  render() {
    const {
      titleSlug,
      isFetching,
      isPopulated
    } = this.props;

    if (!titleSlug) {
      return (
        <NotFound
          message="Sorry, that book cannot be found."
        />
      );
    }

    if ((isFetching || !this.state.hasMounted) ||
        (!isFetching && !isPopulated)) {
      return (
        <PageContent title='loading'>
          <PageContentBody>
            <LoadingIndicator />
          </PageContentBody>
        </PageContent>
      );
    }

    if (!isFetching && isPopulated && this.state.hasMounted) {
      return (
        <BookDetailsConnector
          titleSlug={titleSlug}
        />
      );
    }
  }
}

BookDetailsPageConnector.propTypes = {
  titleSlug: PropTypes.string,
  match: PropTypes.shape({ params: PropTypes.shape({ titleSlug: PropTypes.string.isRequired }).isRequired }).isRequired,
  push: PropTypes.func.isRequired,
  fetchBooks: PropTypes.func.isRequired,
  clearBooks: PropTypes.func.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(BookDetailsPageConnector);
