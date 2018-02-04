import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import connectSection from 'Store/connectSection';
import { createSelector } from 'reselect';
import { fetchReleases, clearReleases, cancelFetchReleases, setReleasesSort, grabRelease } from 'Store/Actions/releaseActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import InteractiveAlbumSearchModalContent from './InteractiveAlbumSearchModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector(),
    createUISettingsSelector(),
    (releases, uiSettings) => {
      return {
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
      dispatch(fetchReleases({ albumId }));
    },

    dispatchCancelFetchReleases() {
      dispatch(cancelFetchReleases());
    },

    dispatchClearReleases() {
      dispatch(clearReleases());
    },

    dispatchSetReleasesSort({ sortKey, sortDirection }) {
      dispatch(setReleasesSort({ sortKey, sortDirection }));
    },

    dispatchGrabRelease({ guid, indexerId }) {
      dispatch(grabRelease({ guid, indexerId }));
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
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.dispatchSetReleasesSort({ sortKey, sortDirection });
  }

  onGrabPress = (guid, indexerId) => {
    this.props.dispatchGrabRelease({ guid, indexerId });
  }

  //
  // Render

  render() {
    return (
      <InteractiveAlbumSearchModalContent
        {...this.props}
        onSortPress={this.onSortPress}
        onGrabPress={this.onGrabPress}
      />
    );
  }
}

InteractiveAlbumSearchModalContentConnector.propTypes = {
  albumId: PropTypes.number,
  dispatchFetchReleases: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchSetReleasesSort: PropTypes.func.isRequired,
  dispatchGrabRelease: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  createMapDispatchToProps,
  undefined,
  undefined,
  { section: 'releases' }
)(InteractiveAlbumSearchModalContentConnector);
