/* eslint max-params: 0 */
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistClientSideCollectionItemsSelector from 'Store/Selectors/createArtistClientSideCollectionItemsSelector';
import dimensions from 'Styles/Variables/dimensions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import scrollPositions from 'Store/scrollPositions';
import { setArtistSort, setArtistFilter, setArtistView, setArtistTableOption } from 'Store/Actions/artistIndexActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import ArtistIndex from './ArtistIndex';

const POSTERS_PADDING = 15;
const POSTERS_PADDING_SMALL_SCREEN = 5;
const BANNERS_PADDING = 15;
const BANNERS_PADDING_SMALL_SCREEN = 5;
const TABLE_PADDING = parseInt(dimensions.pageContentBodyPadding);
const TABLE_PADDING_SMALL_SCREEN = parseInt(dimensions.pageContentBodyPaddingSmallScreen);

// If the scrollTop is greater than zero it needs to be offset
// by the padding so when it is set initially so it is correct
// after React Virtualized takes the padding into account.

function getScrollTop(view, scrollTop, isSmallScreen) {
  if (scrollTop === 0) {
    return 0;
  }

  let padding = isSmallScreen ? TABLE_PADDING_SMALL_SCREEN : TABLE_PADDING;

  if (view === 'posters') {
    padding = isSmallScreen ? POSTERS_PADDING_SMALL_SCREEN : POSTERS_PADDING;
  }

  if (view === 'banners') {
    padding = isSmallScreen ? BANNERS_PADDING_SMALL_SCREEN : BANNERS_PADDING;
  }

  return scrollTop + padding;
}

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
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      view,
      scrollTop,
      isSmallScreen
    } = props;

    this.state = {
      scrollTop: getScrollTop(view, scrollTop, isSmallScreen)
    };
  }

  //
  // Listeners

  onViewSelect = (view) => {
    // Reset the scroll position before changing the view
    this.setState({ scrollTop: 0 }, () => {
      this.props.dispatchSetArtistView(view);
    });
  }

  onScroll = ({ scrollTop }) => {
    this.setState({
      scrollTop
    }, () => {
      scrollPositions.artistIndex = scrollTop;
    });
  }

  //
  // Render

  render() {
    return (
      <ArtistIndex
        {...this.props}
        scrollTop={this.state.scrollTop}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
      />
    );
  }
}

ArtistIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  scrollTop: PropTypes.number.isRequired,
  dispatchSetArtistView: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(ArtistIndexConnector),
  'artistIndex'
);
