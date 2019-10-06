import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAddListMovieSelector from 'Store/Selectors/createAddListMovieSelector';
import createMovieQualityProfileSelector from 'Store/Selectors/createMovieQualityProfileSelector';

function createMapStateToProps() {
  return createSelector(
    createAddListMovieSelector(),
    createMovieQualityProfileSelector(),
    (
      movie,
      qualityProfile
    ) => {

      // If a movie is deleted this selector may fire before the parent
      // selecors, which will result in an undefined movie, if that happens
      // we want to return early here and again in the render function to avoid
      // trying to show a movie that has no information available.

      if (!movie) {
        return {};
      }

      return {
        ...movie,
        qualityProfile
      };
    }
  );
}

const mapDispatchToProps = {
};

class AddListMovieItemConnector extends Component {

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

AddListMovieItemConnector.propTypes = {
  tmdbId: PropTypes.number,
  component: PropTypes.elementType.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddListMovieItemConnector);
