import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import withCurrentPage from 'Components/withCurrentPage';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as queueActions from 'Store/Actions/queueActions';
import { fetchAlbums, clearAlbums } from 'Store/Actions/albumActions';
import * as commandNames from 'Commands/commandNames';
import Queue from './Queue';

function createMapStateToProps() {
  return createSelector(
    (state) => state.albums,
    (state) => state.queue.options,
    (state) => state.queue.paged,
    createCommandExecutingSelector(commandNames.CHECK_FOR_FINISHED_DOWNLOAD),
    (albums, options, queue, isCheckForFinishedDownloadExecuting) => {
      return {
        isAlbumsFetching: albums.isFetching,
        isAlbumsPopulated: albums.isPopulated,
        albumsError: albums.error,
        isCheckForFinishedDownloadExecuting,
        ...options,
        ...queue
      };
    }
  );
}

const mapDispatchToProps = {
  ...queueActions,
  fetchAlbums,
  clearAlbums,
  executeCommand
};

class QueueConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchQueue,
      gotoQueueFirstPage
    } = this.props;

    registerPagePopulator(this.repopulate);

    if (useCurrentPage) {
      fetchQueue();
    } else {
      gotoQueueFirstPage();
    }
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      const albumIds = selectUniqueIds(this.props.items, 'albumId');

      if (albumIds.length) {
        this.props.fetchAlbums({ albumIds });
      } else {
        this.props.clearAlbums();
      }
    }

    if (
      this.props.includeUnknownArtistItems !==
      prevProps.includeUnknownArtistItems
    ) {
      this.repopulate();
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
    this.props.clearQueue();
    this.props.clearAlbums();
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchQueue();
  }

  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoQueueFirstPage();
  }

  onPreviousPagePress = () => {
    this.props.gotoQueuePreviousPage();
  }

  onNextPagePress = () => {
    this.props.gotoQueueNextPage();
  }

  onLastPagePress = () => {
    this.props.gotoQueueLastPage();
  }

  onPageSelect = (page) => {
    this.props.gotoQueuePage({ page });
  }

  onSortPress = (sortKey) => {
    this.props.setQueueSort({ sortKey });
  }

  onTableOptionChange = (payload) => {
    this.props.setQueueTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoQueueFirstPage();
    }
  }

  onRefreshPress = () => {
    this.props.executeCommand({
      name: commandNames.CHECK_FOR_FINISHED_DOWNLOAD
    });
  }

  onGrabSelectedPress = (ids) => {
    this.props.grabQueueItems({ ids });
  }

  onRemoveSelectedPress = (ids, blacklist, skipredownload) => {
    this.props.removeQueueItems({ ids, blacklist, skipredownload });
  }

  //
  // Render

  render() {
    return (
      <Queue
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onSortPress={this.onSortPress}
        onTableOptionChange={this.onTableOptionChange}
        onRefreshPress={this.onRefreshPress}
        onGrabSelectedPress={this.onGrabSelectedPress}
        onRemoveSelectedPress={this.onRemoveSelectedPress}
        {...this.props}
      />
    );
  }
}

QueueConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  includeUnknownArtistItems: PropTypes.bool.isRequired,
  fetchQueue: PropTypes.func.isRequired,
  gotoQueueFirstPage: PropTypes.func.isRequired,
  gotoQueuePreviousPage: PropTypes.func.isRequired,
  gotoQueueNextPage: PropTypes.func.isRequired,
  gotoQueueLastPage: PropTypes.func.isRequired,
  gotoQueuePage: PropTypes.func.isRequired,
  setQueueSort: PropTypes.func.isRequired,
  setQueueTableOption: PropTypes.func.isRequired,
  clearQueue: PropTypes.func.isRequired,
  grabQueueItems: PropTypes.func.isRequired,
  removeQueueItems: PropTypes.func.isRequired,
  fetchAlbums: PropTypes.func.isRequired,
  clearAlbums: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, mapDispatchToProps)(QueueConnector)
);
