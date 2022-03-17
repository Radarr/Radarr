import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import MovieTitlesTableContent from './MovieTitlesTableContent';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    (movie) => {
      let titles = [];

      if (movie.alternateTitles) {
        titles = movie.alternateTitles.map((title) => {
          return {
            id: `title_${title.id}`,
            title: title.title,
            language: title.language || 'Unknown',
            sourceType: 'Alternative Title'
          };
        });
      }

      if (movie.translations) {
        titles = titles.concat(movie.translations.map((title) => {
          return {
            id: `translation_${title.id}`,
            title: title.title,
            language: title.language || 'Unknown',
            sourceType: 'Translation'
          };
        }));
      }

      return {
        titles
      };
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
    const {
      titles
    } = this.props;

    return (
      <MovieTitlesTableContent
        {...this.props}
        titles={titles}
      />
    );
  }
}

MovieTitlesTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  titles: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieTitlesTableContentConnector);
