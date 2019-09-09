import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import withCurrentPage from 'Components/withCurrentPage';
import * as historyActions from 'Store/Actions/historyActions';
import { fetchAlbums, clearAlbums } from 'Store/Actions/albumActions';
import { fetchTracks, clearTracks } from 'Store/Actions/trackActions';
import History from './History';

function createMapStateToProps() {
  return createSelector(
    (state) => state.history,
    (state) => state.albums,
    (state) => state.tracks,
    (history, albums, tracks) => {
      return {
        isAlbumsFetching: albums.isFetching,
        isAlbumsPopulated: albums.isPopulated,
        albumsError: albums.error,
        isTracksFetching: tracks.isFetching,
        isTracksPopulated: tracks.isPopulated,
        tracksError: tracks.error,
        ...history
      };
    }
  );
}

const mapDispatchToProps = {
  ...historyActions,
  fetchAlbums,
  clearAlbums,
  fetchTracks,
  clearTracks
};

class HistoryConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchHistory,
      gotoHistoryFirstPage
    } = this.props;

    registerPagePopulator(this.repopulate);

    if (useCurrentPage) {
      fetchHistory();
    } else {
      gotoHistoryFirstPage();
    }
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      const albumIds = selectUniqueIds(this.props.items, 'albumId');
      const trackIds = selectUniqueIds(this.props.items, 'trackId');
      if (albumIds.length) {
        this.props.fetchAlbums({ albumIds });
      } else {
        this.props.clearAlbums();
      }
      if (trackIds.length) {
        this.props.fetchTracks({ trackIds });
      } else {
        this.props.clearTracks();
      }
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
    this.props.clearHistory();
    this.props.clearAlbums();
    this.props.clearTracks();
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchHistory();
  }

  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoHistoryFirstPage();
  }

  onPreviousPagePress = () => {
    this.props.gotoHistoryPreviousPage();
  }

  onNextPagePress = () => {
    this.props.gotoHistoryNextPage();
  }

  onLastPagePress = () => {
    this.props.gotoHistoryLastPage();
  }

  onPageSelect = (page) => {
    this.props.gotoHistoryPage({ page });
  }

  onSortPress = (sortKey) => {
    this.props.setHistorySort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.setHistoryFilter({ selectedFilterKey });
  }

  onTableOptionChange = (payload) => {
    this.props.setHistoryTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoHistoryFirstPage();
    }
  }

  //
  // Render

  render() {
    return (
      <History
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onTableOptionChange={this.onTableOptionChange}
        {...this.props}
      />
    );
  }
}

HistoryConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchHistory: PropTypes.func.isRequired,
  gotoHistoryFirstPage: PropTypes.func.isRequired,
  gotoHistoryPreviousPage: PropTypes.func.isRequired,
  gotoHistoryNextPage: PropTypes.func.isRequired,
  gotoHistoryLastPage: PropTypes.func.isRequired,
  gotoHistoryPage: PropTypes.func.isRequired,
  setHistorySort: PropTypes.func.isRequired,
  setHistoryFilter: PropTypes.func.isRequired,
  setHistoryTableOption: PropTypes.func.isRequired,
  clearHistory: PropTypes.func.isRequired,
  fetchAlbums: PropTypes.func.isRequired,
  clearAlbums: PropTypes.func.isRequired,
  fetchTracks: PropTypes.func.isRequired,
  clearTracks: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, mapDispatchToProps)(HistoryConnector)
);
