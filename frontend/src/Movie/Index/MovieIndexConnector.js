import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import { executeCommand } from 'Store/Actions/commandActions';
import { saveMovieEditor, setMovieFilter, setMovieSort, setMovieTableOption, setMovieView } from 'Store/Actions/movieIndexActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import scrollPositions from 'Store/scrollPositions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createMovieClientSideCollectionItemsSelector from 'Store/Selectors/createMovieClientSideCollectionItemsSelector';
import MovieIndex from './MovieIndex';

function createMapStateToProps() {
  return createSelector(
    createMovieClientSideCollectionItemsSelector('movieIndex'),
    createCommandExecutingSelector(commandNames.REFRESH_MOVIE),
    createCommandExecutingSelector(commandNames.RSS_SYNC),
    createCommandExecutingSelector(commandNames.RENAME_MOVIE),
    createCommandExecutingSelector(commandNames.CUTOFF_UNMET_MOVIES_SEARCH),
    createCommandExecutingSelector(commandNames.MISSING_MOVIES_SEARCH),
    createDimensionsSelector(),
    (
      movies,
      isRefreshingMovie,
      isRssSyncExecuting,
      isOrganizingMovie,
      isCutoffMoviesSearch,
      isMissingMoviesSearch,
      dimensionsState
    ) => {
      return {
        ...movies,
        isRefreshingMovie,
        isRssSyncExecuting,
        isOrganizingMovie,
        isSearchingMovies: isCutoffMoviesSearch || isMissingMoviesSearch,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    fetchQueueDetails() {
      dispatch(fetchQueueDetails());
    },

    clearQueueDetails() {
      dispatch(clearQueueDetails());
    },

    dispatchFetchRootFolders() {
      dispatch(fetchRootFolders());
    },

    onTableOptionChange(payload) {
      dispatch(setMovieTableOption(payload));
    },

    onSortSelect(sortKey) {
      dispatch(setMovieSort({ sortKey }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(setMovieFilter({ selectedFilterKey }));
    },

    dispatchSetMovieView(view) {
      dispatch(setMovieView({ view }));
    },

    dispatchSaveMovieEditor(payload) {
      dispatch(saveMovieEditor(payload));
    },

    onRefreshMoviePress(items) {
      dispatch(executeCommand({
        name: commandNames.REFRESH_MOVIE,
        movieIds: items
      }));
    },

    onRssSyncPress() {
      dispatch(executeCommand({
        name: commandNames.RSS_SYNC
      }));
    },

    onSearchPress(command, items) {
      dispatch(executeCommand({
        name: command,
        movieIds: items
      }));
    }
  };
}

class MovieIndexConnector extends Component {

  componentDidMount() {
    // TODO: Fetch root folders here for now, but should eventually fetch on editor toggle and check loaded before showing controls
    this.props.dispatchFetchRootFolders();
    this.props.fetchQueueDetails();
  }

  componentWillUnmount() {
    this.props.clearQueueDetails();
  }

  //
  // Listeners

  onViewSelect = (view) => {
    // Reset the scroll position before changing the view
    this.props.dispatchSetMovieView(view);
  };

  onSaveSelected = (payload) => {
    this.props.dispatchSaveMovieEditor(payload);
  };

  onScroll = ({ scrollTop }) => {
    scrollPositions.movieIndex = scrollTop;
  };

  //
  // Render

  render() {
    return (
      <MovieIndex
        {...this.props}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
        onSaveSelected={this.onSaveSelected}
      />
    );
  }
}

MovieIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchSetMovieView: PropTypes.func.isRequired,
  dispatchSaveMovieEditor: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  items: PropTypes.arrayOf(PropTypes.object)
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(MovieIndexConnector),
  'movieIndex'
);
