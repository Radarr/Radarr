import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { toggleCollectionMonitored } from 'Store/Actions/movieCollectionActions';
import createCollectionSelector from 'Store/Selectors/createCollectionSelector';
import MovieCollectionLabel from './MovieCollectionLabel';

function createMapStateToProps() {
  return createSelector(
    createCollectionSelector(),
    (collection) => {
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
      collectionId: this.props.collectionId,
      monitored
    });
  }

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
  collectionId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  toggleCollectionMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCollectionLabelConnector);
