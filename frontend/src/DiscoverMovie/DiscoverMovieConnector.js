import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import { executeCommand } from 'Store/Actions/commandActions';
import { addImportExclusions, addMovies, clearAddMovie, fetchDiscoverMovies, setListMovieFilter, setListMovieSort, setListMovieTableOption, setListMovieView } from 'Store/Actions/discoverMovieActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchImportExclusions } from 'Store/Actions/Settings/importExclusions';
import scrollPositions from 'Store/scrollPositions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createDiscoverMovieClientSideCollectionItemsSelector from 'Store/Selectors/createDiscoverMovieClientSideCollectionItemsSelector';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import DiscoverMovie from './DiscoverMovie';

function createMapStateToProps() {
  return createSelector(
    createDiscoverMovieClientSideCollectionItemsSelector('discoverMovie'),
    createCommandExecutingSelector(commandNames.IMPORT_LIST_SYNC),
    createDimensionsSelector(),
    (
      movies,
      isSyncingLists,
      dimensionsState
    ) => {
      return {
        ...movies,
        isSyncingLists,
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

    dispatchFetchImportExclusions() {
      dispatch(fetchImportExclusions());
    },

    dispatchClearListMovie() {
      dispatch(clearAddMovie());
    },

    dispatchFetchListMovies() {
      dispatch(fetchDiscoverMovies());
    },

    onTableOptionChange(payload) {
      dispatch(setListMovieTableOption(payload));
    },

    onSortSelect(sortKey) {
      dispatch(setListMovieSort({ sortKey }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(setListMovieFilter({ selectedFilterKey }));
    },

    dispatchSetListMovieView(view) {
      dispatch(setListMovieView({ view }));
    },

    dispatchAddMovies(ids, addOptions) {
      dispatch(addMovies({ ids, addOptions }));
    },

    dispatchAddImportExclusions(exclusions) {
      dispatch(addImportExclusions(exclusions));
    },

    onImportListSyncPress() {
      dispatch(executeCommand({
        name: commandNames.IMPORT_LIST_SYNC
      }));
    }
  };
}

class DiscoverMovieConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.repopulate);
    this.props.dispatchFetchRootFolders();
    this.props.dispatchFetchImportExclusions();
    this.props.dispatchFetchListMovies();
  }

  componentWillUnmount() {
    this.props.dispatchClearListMovie();
    unregisterPagePopulator(this.repopulate);
  }

  //
  // Listeners

  onViewSelect = (view) => {
    this.props.dispatchSetListMovieView(view);
  };

  onScroll = ({ scrollTop }) => {
    scrollPositions.discoverMovie = scrollTop;
  };

  onAddMoviesPress = ({ ids, addOptions }) => {
    this.props.dispatchAddMovies(ids, addOptions);
  };

  onExcludeMoviesPress =({ ids }) => {
    this.props.dispatchAddImportExclusions({ ids });
  };

  //
  // Render

  render() {
    return (
      <DiscoverMovie
        {...this.props}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
        onAddMoviesPress={this.onAddMoviesPress}
        onExcludeMoviesPress={this.onExcludeMoviesPress}
        onSyncListsPress={this.onSyncListsPress}
      />
    );
  }
}

DiscoverMovieConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  dispatchFetchImportExclusions: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchFetchListMovies: PropTypes.func.isRequired,
  dispatchClearListMovie: PropTypes.func.isRequired,
  dispatchSetListMovieView: PropTypes.func.isRequired,
  dispatchAddMovies: PropTypes.func.isRequired,
  dispatchAddImportExclusions: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(DiscoverMovieConnector),
  'discoverMovie'
);
