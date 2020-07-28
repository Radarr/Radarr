import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import withScrollPosition from 'Components/withScrollPosition';
import { addMovies, addNetImportExclusions, clearAddMovie, fetchDiscoverMovies, setListMovieFilter, setListMovieSort, setListMovieTableOption, setListMovieView } from 'Store/Actions/discoverMovieActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchNetImportExclusions } from 'Store/Actions/Settings/netImportExclusions';
import scrollPositions from 'Store/scrollPositions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createDiscoverMovieClientSideCollectionItemsSelector from 'Store/Selectors/createDiscoverMovieClientSideCollectionItemsSelector';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import DiscoverMovie from './DiscoverMovie';

function createMapStateToProps() {
  return createSelector(
    createDiscoverMovieClientSideCollectionItemsSelector('discoverMovie'),
    createDimensionsSelector(),
    (
      movies,
      dimensionsState
    ) => {
      return {
        ...movies,
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

    dispatchFetchNetImportExclusions() {
      dispatch(fetchNetImportExclusions());
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

    dispatchAddNetImportExclusions(exclusions) {
      dispatch(addNetImportExclusions(exclusions));
    }
  };
}

class DiscoverMovieConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.repopulate);
    this.props.dispatchFetchRootFolders();
    this.props.dispatchFetchNetImportExclusions();
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
  }

  onScroll = ({ scrollTop }) => {
    scrollPositions.discoverMovie = scrollTop;
  }

  onAddMoviesPress = ({ ids, addOptions }) => {
    this.props.dispatchAddMovies(ids, addOptions);
  }

  onExcludeMoviesPress =({ ids }) => {
    this.props.dispatchAddNetImportExclusions({ ids });
  }

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
      />
    );
  }
}

DiscoverMovieConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  dispatchFetchNetImportExclusions: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchFetchListMovies: PropTypes.func.isRequired,
  dispatchClearListMovie: PropTypes.func.isRequired,
  dispatchSetListMovieView: PropTypes.func.isRequired,
  dispatchAddMovies: PropTypes.func.isRequired,
  dispatchAddNetImportExclusions: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(DiscoverMovieConnector),
  'discoverMovie'
);
