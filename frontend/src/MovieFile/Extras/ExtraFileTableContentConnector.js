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
      const filesForMovie = extraFiles.items.filter((obj) => {
        return obj.movieId === movieId;
      });

      return {
        items: filesForMovie,
        error: null
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
  };
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

export default connect(createMapStateToProps, createMapDispatchToProps)(ExtraFileTableContentConnector);
