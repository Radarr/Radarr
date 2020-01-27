import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAddMovieClientSideCollectionItemsSelector from 'Store/Selectors/createAddMovieClientSideCollectionItemsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchDiscoverMovies, clearAddMovie, setListMovieSort, setListMovieFilter, setListMovieView, setListMovieTableOption } from 'Store/Actions/addMovieActions';
import scrollPositions from 'Store/scrollPositions';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import withScrollPosition from 'Components/withScrollPosition';
import AddListMovie from './AddListMovie';

function createMapStateToProps() {
  return createSelector(
    createAddMovieClientSideCollectionItemsSelector('addMovie'),
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
    }
  };
}

class AddDiscoverMovieConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.repopulate);
    this.props.dispatchFetchRootFolders();
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
    scrollPositions.addMovie = scrollTop;
  }

  //
  // Render

  render() {
    return (
      <AddListMovie
        {...this.props}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
        onSaveSelected={this.onSaveSelected}
      />
    );
  }
}

AddDiscoverMovieConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchFetchListMovies: PropTypes.func.isRequired,
  dispatchClearListMovie: PropTypes.func.isRequired,
  dispatchSetListMovieView: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(AddDiscoverMovieConnector),
  'addMovie'
);
