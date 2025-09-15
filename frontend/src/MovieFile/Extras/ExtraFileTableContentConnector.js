import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import ExtraFileTableContent from './ExtraFileTableContent';

function createMapStateToProps() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.extraFiles,
    createMovieSelector(),
    (
      movieId,
      extraFiles
    ) => {
      const filesForMovie = extraFiles.items.filter((file) => file.movieId === movieId).reduce((acc, file) => {
        if (!acc.some((f) => f.relativePath === file.relativePath)) {
          acc.push(file);
        }
        return acc;
      }, []);

      return {
        items: filesForMovie,
        error: null
      };
    }
  );
}

class ExtraFileTableContentConnector extends Component {

  //
  // Render

  render() {
    const {
      ...otherProps
    } = this.props;

    return (
      <ExtraFileTableContent
        {...otherProps}
      />
    );
  }
}

ExtraFileTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default connect(createMapStateToProps, null)(ExtraFileTableContentConnector);
