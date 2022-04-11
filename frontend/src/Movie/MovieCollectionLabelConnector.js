import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { toggleCollectionMonitored } from 'Store/Actions/movieCollectionActions';
import MovieCollectionLabel from './MovieCollectionLabel';

function createMapStateToProps() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.movieCollections.items,
    (tmdbId, collections) => {
      const collection = collections.find((movie) => movie.tmdbId === tmdbId);
      return {
        ...collection
      };
    }
  );
}

const mapDispatchToProps = {
  toggleCollectionMonitored
};

class MovieCollectionLabelConnector extends Component {

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleCollectionMonitored({
      collectionId: this.props.id,
      monitored
    });
  };

  //
  // Render

  render() {
    return (
      <MovieCollectionLabel
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
      />
    );
  }
}

MovieCollectionLabelConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  id: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  toggleCollectionMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCollectionLabelConnector);
