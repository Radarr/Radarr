import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as releaseActions from 'Store/Actions/releaseActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import InteractiveSearchContent from './InteractiveSearchContent';

function createMapStateToProps(appState) {
  return createSelector(
    (state) => state.releases.items.length,
    createClientSideCollectionSelector('releases'),
    createUISettingsSelector(),
    (totalReleasesCount, releases, uiSettings) => {
      return {
        totalReleasesCount,
        longDateFormat: uiSettings.longDateFormat,
        timeFormat: uiSettings.timeFormat,
        ...releases
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchReleases(payload) {
      dispatch(releaseActions.fetchReleases(payload));
    },

    dispatchClearReleases(payload) {
      dispatch(releaseActions.clearReleases(payload));
    },

    onSortPress(sortKey, sortDirection) {
      dispatch(releaseActions.setReleasesSort({ sortKey, sortDirection }));
    },

    onFilterSelect(selectedFilterKey) {
      const action = releaseActions.setReleasesFilter;
      dispatch(action({ selectedFilterKey }));
    },

    onGrabPress(payload) {
      dispatch(releaseActions.grabRelease(payload));
    }
  };
}

class InteractiveSearchContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      searchPayload,
      isPopulated,
      dispatchFetchReleases
    } = this.props;

    // If search results are not yet isPopulated fetch them,
    // otherwise re-show the existing props.
    if (!isPopulated) {
      dispatchFetchReleases(searchPayload);
    }
  }

  componentWillUnmount() {
    const {
      dispatchClearReleases
    } = this.props;
    dispatchClearReleases();
  }

  //
  // Render

  render() {
    const {
      dispatchFetchReleases,
      dispatchClearReleases,
      ...otherProps
    } = this.props;

    return (

      <InteractiveSearchContent
        {...otherProps}
      />
    );
  }
}

InteractiveSearchContentConnector.propTypes = {
  searchPayload: PropTypes.object.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  dispatchFetchReleases: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(InteractiveSearchContentConnector);
