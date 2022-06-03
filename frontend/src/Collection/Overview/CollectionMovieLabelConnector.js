import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import createCollectionExistingMovieSelector from 'Store/Selectors/createCollectionExistingMovieSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import CollectionMovieLabel from './CollectionMovieLabel';

function createMapStateToProps() {
  return createSelector(
    createDimensionsSelector(),
    createCollectionExistingMovieSelector(),
    (dimensions, existingMovie) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        isExistingMovie: !!existingMovie,
        ...existingMovie
      };
    }
  );
}

const mapDispatchToProps = {
  toggleMovieMonitored
};

class CollectionMovieLabelConnector extends Component {

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleMovieMonitored({
      movieId: this.props.id,
      monitored
    });
  };

  //
  // Render

  render() {
    return (
      <CollectionMovieLabel
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
      />
    );
  }
}

CollectionMovieLabelConnector.propTypes = {
  id: PropTypes.number,
  monitored: PropTypes.bool,
  toggleMovieMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(CollectionMovieLabelConnector);
