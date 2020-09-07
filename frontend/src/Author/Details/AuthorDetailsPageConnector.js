import { push } from 'connected-react-router';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import NotFound from 'Components/NotFound';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import AuthorDetailsConnector from './AuthorDetailsConnector';
import styles from './AuthorDetails.css';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.authors,
    (match, authors) => {
      const titleSlug = match.params.titleSlug;
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = authors;

      const authorIndex = _.findIndex(items, { titleSlug });

      if (authorIndex > -1) {
        return {
          isFetching,
          isPopulated,
          titleSlug
        };
      }

      return {
        isFetching,
        isPopulated,
        error
      };
    }
  );
}

const mapDispatchToProps = {
  push
};

class AuthorDetailsPageConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps) {
    if (!this.props.titleSlug) {
      this.props.push(`${window.Readarr.urlBase}/`);
      return;
    }
  }

  //
  // Render

  render() {
    const {
      titleSlug,
      isFetching,
      isPopulated,
      error
    } = this.props;

    if (isFetching && !isPopulated) {
      return (
        <PageContent title='loading'>
          <PageContentBodyConnector>
            <LoadingIndicator />
          </PageContentBodyConnector>
        </PageContent>
      );
    }

    if (!isFetching && !!error) {
      return (
        <div className={styles.errorMessage}>
          {getErrorMessage(error, 'Failed to load author from API')}
        </div>
      );
    }

    if (!titleSlug) {
      return (
        <NotFound
          message="Sorry, that author cannot be found."
        />
      );
    }

    return (
      <AuthorDetailsConnector
        titleSlug={titleSlug}
      />
    );
  }
}

AuthorDetailsPageConnector.propTypes = {
  titleSlug: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  match: PropTypes.shape({ params: PropTypes.shape({ titleSlug: PropTypes.string.isRequired }).isRequired }).isRequired,
  push: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AuthorDetailsPageConnector);
