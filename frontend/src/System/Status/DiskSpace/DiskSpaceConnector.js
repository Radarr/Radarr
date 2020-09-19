import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchDiskSpace } from 'Store/Actions/systemActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import DiskSpace from './DiskSpace';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.diskSpace,
    createDimensionsSelector(),
    (diskSpace, dimensions) => {
      const {
        isFetching,
        items
      } = diskSpace;

      return {
        isFetching,
        items,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

const mapDispatchToProps = {
  fetchDiskSpace
};

class DiskSpaceConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchDiskSpace();
  }

  //
  // Render

  render() {
    return (
      <DiskSpace
        {...this.props}
      />
    );
  }
}

DiskSpaceConnector.propTypes = {
  fetchDiskSpace: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DiskSpaceConnector);
