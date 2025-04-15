import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDiscoverMovieSelector from 'Store/Selectors/createDiscoverMovieSelector';

function createMapStateToProps() {
  return createSelector(
    createDiscoverMovieSelector(),
    (
      movie
    ) => {

      // If a movie is deleted this selector may fire before the parent
      // selectors, which will result in an undefined movie, if that happens
      // we want to return early here and again in the render function to avoid
      // trying to show a movie that has no information available.

      if (!movie) {
        return {};
      }

      return {
        ...movie
      };
    }
  );
}

class DiscoverMovieItemConnector extends Component {

  //
  // Render

  render() {
    const {
      tmdbId,
      component: ItemComponent,
      ...otherProps
    } = this.props;

    if (!tmdbId) {
      return null;
    }

    return (
      <ItemComponent
        {...otherProps}
        tmdbId={tmdbId}
      />
    );
  }
}

DiscoverMovieItemConnector.propTypes = {
  tmdbId: PropTypes.number,
  component: PropTypes.elementType.isRequired
};

export default connect(createMapStateToProps)(DiscoverMovieItemConnector);
