/* eslint max-params: 0 */
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistClientSideCollectionItemsSelector from 'Store/Selectors/createArtistClientSideCollectionItemsSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import scrollPositions from 'Store/scrollPositions';
import { setArtistSort, setArtistFilter, setArtistView, setArtistTableOption } from 'Store/Actions/artistIndexActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import ArtistIndex from './ArtistIndex';

function createMapStateToProps() {
  return createSelector(
    createArtistClientSideCollectionItemsSelector('artistIndex'),
    createCommandExecutingSelector(commandNames.REFRESH_ARTIST),
    createCommandExecutingSelector(commandNames.RSS_SYNC),
    createDimensionsSelector(),
    (
      artist,
      isRefreshingArtist,
      isRssSyncExecuting,
      dimensionsState
    ) => {
      return {
        ...artist,
        isRefreshingArtist,
        isRssSyncExecuting,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setArtistTableOption(payload));
    },

    onSortSelect(sortKey) {
      dispatch(setArtistSort({ sortKey }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(setArtistFilter({ selectedFilterKey }));
    },

    dispatchSetArtistView(view) {
      dispatch(setArtistView({ view }));
    },

    onRefreshArtistPress() {
      dispatch(executeCommand({
        name: commandNames.REFRESH_ARTIST
      }));
    },

    onRssSyncPress() {
      dispatch(executeCommand({
        name: commandNames.RSS_SYNC
      }));
    }
  };
}

class ArtistIndexConnector extends Component {

  //
  // Listeners

  onViewSelect = (view) => {
    this.props.dispatchSetArtistView(view);
  }

  onScroll = ({ scrollTop }) => {
    scrollPositions.artistIndex = scrollTop;
  }

  //
  // Render

  render() {
    return (
      <ArtistIndex
        {...this.props}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
      />
    );
  }
}

ArtistIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  dispatchSetArtistView: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(ArtistIndexConnector),
  'artistIndex'
);
