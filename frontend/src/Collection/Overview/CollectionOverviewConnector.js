import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { toggleCollectionMonitored } from 'Store/Actions/movieCollectionActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import CollectionOverview from './CollectionOverview';

function createMapStateToProps() {
  return createSelector(
    createDimensionsSelector(),
    (dimensions) => {
      return {
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

const mapDispatchToProps = {
  toggleCollectionMonitored
};

class CollectionOverviewConnector extends Component {

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleCollectionMonitored({
      collectionId: this.props.collectionId,
      monitored
    });
  };

  //
  // Render

  render() {
    return (
      <CollectionOverview
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
      />
    );
  }
}

CollectionOverviewConnector.propTypes = {
  collectionId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  toggleCollectionMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(CollectionOverviewConnector);
