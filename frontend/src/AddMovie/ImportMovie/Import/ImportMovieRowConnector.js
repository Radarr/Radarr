import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupMovie, setImportMovieValue } from 'Store/Actions/importMovieActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import ImportMovieRow from './ImportMovieRow';

function createImportMovieItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.importMovie.items,
    (id, items) => {
      return _.find(items, { id }) || {};
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createImportMovieItemSelector(),
    createAllMoviesSelector(),
    (item, movies) => {
      const selectedMovie = item && item.selectedMovie;
      const isExistingMovie = !!selectedMovie && _.some(movies, { tmdbId: selectedMovie.tmdbId });

      return {
        ...item,
        isExistingMovie
      };
    }
  );
}

const mapDispatchToProps = {
  queueLookupMovie,
  setImportMovieValue
};

class ImportMovieRowConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setImportMovieValue({
      id: this.props.id,
      [name]: value
    });
  };

  //
  // Render

  render() {
    // Don't show the row until we have the information we require for it.

    const {
      items,
      monitor
    } = this.props;

    if (!items || !monitor) {
      return null;
    }

    return (
      <ImportMovieRow
        {...this.props}
        onInputChange={this.onInputChange}
        onMovieSelect={this.onMovieSelect}
      />
    );
  }
}

ImportMovieRowConnector.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  id: PropTypes.string.isRequired,
  monitor: PropTypes.string,
  items: PropTypes.arrayOf(PropTypes.object),
  queueLookupMovie: PropTypes.func.isRequired,
  setImportMovieValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportMovieRowConnector);
