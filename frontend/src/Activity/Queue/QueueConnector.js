import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as queueActions from 'Store/Actions/queueActions';
import { fetchAlbums, clearAlbums } from 'Store/Actions/albumActions';
import * as commandNames from 'Commands/commandNames';
import Queue from './Queue';

function createMapStateToProps() {
  return createSelector(
    (state) => state.albums,
    (state) => state.queue.paged,
    createCommandExecutingSelector(commandNames.CHECK_FOR_FINISHED_DOWNLOAD),
    (albums, queue, isCheckForFinishedDownloadExecuting) => {
      return {
        isAlbumsFetching: albums.isFetching,
        isAlbumsPopulated: albums.isPopulated,
        albumsError: albums.error,
        isCheckForFinishedDownloadExecuting,
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
    registerPagePopulator(this.repopulate);
    this.props.gotoQueueFirstPage();
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

  onRemoveSelectedPress = (ids, blacklist) => {
    this.props.removeQueueItems({ ids, blacklist });
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
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
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

export default connect(createMapStateToProps, mapDispatchToProps)(QueueConnector);
