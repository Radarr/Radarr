import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
// import { fetchMovies  } from 'Store/Actions/movieTitlesActions';
import MovieTitlesTableContent from './MovieTitlesTableContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movies,
    (movies) => {
      return movies;
    }
  );
}

const mapDispatchToProps = {
//  fetchMovies
};

class MovieTitlesTableContentConnector extends Component {

  //
  // Render

  render() {
    var result = this.props.items.filter(obj => {
      return obj.id === this.props.movieId
    })

    return (
      <MovieTitlesTableContent
        {...this.props}
        items={result[0].alternateTitles}
      />
    );
  }
}

MovieTitlesTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieTitlesTableContentConnector);
