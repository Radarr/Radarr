import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setReleasesFilter } from 'Store/Actions/releaseActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import InteractiveSearchFilterMenu from './InteractiveSearchFilterMenu';

function createMapStateToProps(appState) {
  return createSelector(
    createClientSideCollectionSelector('releases'),
    (releases) => {
      return {
        ...releases
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onFilterSelect(selectedFilterKey) {
      dispatch(setReleasesFilter({ selectedFilterKey }));
    }
  };
}

class InteractiveSearchFilterMenuConnector extends Component {

  //
  // Render

  render() {
    const {
      ...otherProps
    } = this.props;

    return (

      <InteractiveSearchFilterMenu
        {...otherProps}
      />
    );
  }
}

export default connect(createMapStateToProps, createMapDispatchToProps)(InteractiveSearchFilterMenuConnector);
