import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
//import { fetchMovies  } from 'Store/Actions/movieTitlesActions';
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
  // Lifecycle

  componentDidMount() {
    // const {
    //   movieId
    // } = this.props;

    // this.props.fetchMovies({
    //   movieId
    // });
  }

  componentWillUnmount() {
  }

  //
  // Listeners

  //
  // Render

  render() {
    return (
      <MovieTitlesTableContent
        {...this.props}
      />
    );
  }
}

MovieTitlesTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
//  fetchMovieTitles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieTitlesTableContentConnector);
