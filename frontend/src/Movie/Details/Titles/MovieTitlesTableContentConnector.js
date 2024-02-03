import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieTitlesTableContent from './MovieTitlesTableContent';

function createMapStateToProps() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.movies,
    (movieId, movies) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = movies;

      const alternateTitles = items.find((m) => m.id === movieId)?.alternateTitles;

      return {
        isFetching,
        isPopulated,
        error,
        alternateTitles
      };
    }
  );
}

class MovieTitlesTableContentConnector extends Component {

  //
  // Render

  render() {
    const {
      alternateTitles,
      ...otherProps
    } = this.props;

    return (
      <MovieTitlesTableContent
        {...otherProps}
        items={alternateTitles}
      />
    );
  }
}

MovieTitlesTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired
};

MovieTitlesTableContentConnector.defaultProps = {
  alternateTitles: []
};

export default connect(createMapStateToProps)(MovieTitlesTableContentConnector);
