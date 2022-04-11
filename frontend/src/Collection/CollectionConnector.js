import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import { executeCommand } from 'Store/Actions/commandActions';
import { saveMovieCollections, setMovieCollectionsFilter, setMovieCollectionsSort } from 'Store/Actions/movieCollectionActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import scrollPositions from 'Store/scrollPositions';
import createCollectionClientSideCollectionItemsSelector from 'Store/Selectors/createCollectionClientSideCollectionItemsSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import Collection from './Collection';

function createMapStateToProps() {
  return createSelector(
    createCollectionClientSideCollectionItemsSelector('movieCollections'),
    createCommandExecutingSelector(commandNames.REFRESH_COLLECTIONS),
    createDimensionsSelector(),
    (
      collections,
      isRefreshingCollections,
      dimensionsState
    ) => {
      return {
        ...collections,
        isRefreshingCollections,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchRootFolders() {
      dispatch(fetchRootFolders());
    },
    onUpdateSelectedPress(payload) {
      dispatch(saveMovieCollections(payload));
    },
    onSortSelect(sortKey) {
      dispatch(setMovieCollectionsSort({ sortKey }));
    },
    onFilterSelect(selectedFilterKey) {
      dispatch(setMovieCollectionsFilter({ selectedFilterKey }));
    },
    onRefreshMovieCollectionsPress() {
      dispatch(executeCommand({
        name: commandNames.REFRESH_COLLECTIONS
      }));
    }
  };
}

class CollectionConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.repopulate);
    this.props.dispatchFetchRootFolders();
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
  }

  //
  // Listeners

  onScroll = ({ scrollTop }) => {
    scrollPositions.movieCollections = scrollTop;
  };

  onUpdateSelectedPress = (payload) => {
    this.props.onUpdateSelectedPress(payload);
  };

  //
  // Render

  render() {
    return (
      <Collection
        {...this.props}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
        onUpdateSelectedPress={this.onUpdateSelectedPress}
      />
    );
  }
}

CollectionConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  onUpdateSelectedPress: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(CollectionConnector),
  'movieCollections'
);
