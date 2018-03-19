import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import * as releaseActions from 'Store/Actions/releaseActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import InteractiveAlbumSearchModalContent from './InteractiveAlbumSearchModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.releases.items.length,
    createClientSideCollectionSelector(),
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
    dispatchFetchReleases({ albumId }) {
      dispatch(releaseActions.fetchReleases({ albumId }));
    },

    dispatchCancelFetchReleases() {
      dispatch(releaseActions.cancelFetchReleases());
    },

    dispatchClearReleases() {
      dispatch(releaseActions.clearReleases());
    },

    onSortPress(sortKey, sortDirection) {
      dispatch(releaseActions.setReleasesSort({ sortKey, sortDirection }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(releaseActions.setReleasesFilter({ selectedFilterKey }));
    },

    onGrabPress(guid, indexerId) {
      dispatch(releaseActions.grabRelease({ guid, indexerId }));
    }
  };
}

class InteractiveAlbumSearchModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      albumId
    } = this.props;

    this.props.dispatchFetchReleases({
      albumId
    });
  }

  componentWillUnmount() {
    this.props.dispatchCancelFetchReleases();
    this.props.dispatchClearReleases();
  }

  //
  // Render

  render() {
    const {
      dispatchFetchReleases,
      ...otherProps
    } = this.props;

    return (
      <InteractiveAlbumSearchModalContent
        {...otherProps}
      />
    );
  }
}

InteractiveAlbumSearchModalContentConnector.propTypes = {
  albumId: PropTypes.number,
  dispatchFetchReleases: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  createMapDispatchToProps,
  undefined,
  undefined,
  { section: 'releases' }
)(InteractiveAlbumSearchModalContentConnector);
