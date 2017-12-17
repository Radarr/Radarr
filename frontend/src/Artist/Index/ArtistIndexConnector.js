/* eslint max-params: 0 */
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import dimensions from 'Styles/Variables/dimensions';
import createCommandSelector from 'Store/Selectors/createCommandSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { fetchArtist } from 'Store/Actions/artistActions';
import scrollPositions from 'Store/scrollPositions';
import { setArtistSort, setArtistFilter, setArtistView } from 'Store/Actions/artistIndexActions';
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
    (state) => state.artist,
    (state) => state.artistIndex,
    createCommandSelector(commandNames.REFRESH_ARTIST),
    createCommandSelector(commandNames.RSS_SYNC),
    createDimensionsSelector(),
    (artist, artistIndex, isRefreshingArtist, isRssSyncExecuting, dimensionsState) => {
      return {
        isRefreshingArtist,
        isRssSyncExecuting,
        isSmallScreen: dimensionsState.isSmallScreen,
        ...artist,
        ...artistIndex
      };
    }
  );
}

const mapDispatchToProps = {
  fetchArtist,
  setArtistSort,
  setArtistFilter,
  setArtistView,
  executeCommand
};

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

  componentDidMount() {
    this.props.fetchArtist();
  }

  //
  // Listeners

  onSortSelect = (sortKey) => {
    this.props.setArtistSort({ sortKey });
  }

  onFilterSelect = (filterKey, filterValue, filterType) => {
    this.props.setArtistFilter({ filterKey, filterValue, filterType });
  }

  onViewSelect = (view) => {
    // Reset the scroll position before changing the view
    this.setState({ scrollTop: 0 }, () => {
      this.props.setArtistView({ view });
    });
  }

  onScroll = ({ scrollTop }) => {
    this.setState({
      scrollTop
    }, () => {
      scrollPositions.artistIndex = scrollTop;
    });
  }

  onRefreshArtistPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_ARTIST
    });
  }

  onRssSyncPress = () => {
    this.props.executeCommand({
      name: commandNames.RSS_SYNC
    });
  }

  //
  // Render

  render() {
    return (
      <ArtistIndex
        {...this.props}
        scrollTop={this.state.scrollTop}
        onSortSelect={this.onSortSelect}
        onFilterSelect={this.onFilterSelect}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
        onRefreshArtistPress={this.onRefreshArtistPress}
        onRssSyncPress={this.onRssSyncPress}
      />
    );
  }
}

ArtistIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  scrollTop: PropTypes.number.isRequired,
  fetchArtist: PropTypes.func.isRequired,
  setArtistSort: PropTypes.func.isRequired,
  setArtistFilter: PropTypes.func.isRequired,
  setArtistView: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, mapDispatchToProps)(ArtistIndexConnector),
  'artistIndex'
);
