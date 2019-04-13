import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { push } from 'connected-react-router';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import NotFound from 'Components/NotFound';
import MovieDetailsConnector from './MovieDetailsConnector';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    createAllMoviesSelector(),
    (match, allMovies) => {
      const titleSlug = match.params.titleSlug;
      const movieIndex = _.findIndex(allMovies, { titleSlug });

      if (movieIndex > -1) {
        return {
          titleSlug
        };
      }

      return {};
    }
  );
}

const mapDispatchToProps = {
  push
};

class MovieDetailsPageConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps) {
    if (!this.props.titleSlug) {
      this.props.push(`${window.Radarr.urlBase}/`);
      return;
    }
  }

  //
  // Render

  render() {
    const {
      titleSlug
    } = this.props;

    if (!titleSlug) {
      return (
        <NotFound
          message="Sorry, that series cannot be found."
        />
      );
    }

    return (
      <MovieDetailsConnector
        titleSlug={titleSlug}
      />
    );
  }
}

MovieDetailsPageConnector.propTypes = {
  titleSlug: PropTypes.string,
  match: PropTypes.shape({ params: PropTypes.shape({ titleSlug: PropTypes.string.isRequired }).isRequired }).isRequired,
  push: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieDetailsPageConnector);
