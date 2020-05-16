import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as releaseActions from 'Store/Actions/releaseActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import InteractiveSearchFilterMenu from './InteractiveSearchFilterMenu';

function createMapStateToProps(appState, { type }) {
  return createSelector(
    createClientSideCollectionSelector('releases', `releases.${type}`),
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
      const action = props.type === 'book' ?
        releaseActions.setBookReleasesFilter :
        releaseActions.setAuthorReleasesFilter;
      dispatch(action({ selectedFilterKey }));
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
