import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createCollectionSelector from 'Store/Selectors/createCollectionSelector';

function createMapStateToProps() {
  return createSelector(
    createCollectionSelector(),
    (collection) => {
      // If a movie is deleted this selector may fire before the parent
      // selectors, which will result in an undefined movie, if that happens
      // we want to return early here and again in the render function to avoid
      // trying to show a movie that has no information available.

      if (!collection) {
        return {};
      }

      const allGenres = collection.movies.flatMap((movie) => movie.genres);

      return {
        ...collection,
        movies: [...collection.movies].sort((a, b) => b.year - a.year),
        genres: Array.from(new Set(allGenres))
      };
    }
  );
}

class CollectionItemConnector extends Component {

  //
  // Render

  render() {
    const {
      id,
      component: ItemComponent,
      ...otherProps
    } = this.props;

    if (!id) {
      return null;
    }

    return (
      <ItemComponent
        {...otherProps}
        id={id}
      />
    );
  }
}

CollectionItemConnector.propTypes = {
  id: PropTypes.number,
  component: PropTypes.elementType.isRequired
};

export default connect(createMapStateToProps)(CollectionItemConnector);
