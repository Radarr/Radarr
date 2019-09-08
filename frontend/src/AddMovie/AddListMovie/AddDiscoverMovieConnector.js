import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import dimensions from 'Styles/Variables/dimensions';
import createAddMovieClientSideCollectionItemsSelector from 'Store/Selectors/createAddMovieClientSideCollectionItemsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchDiscoverMovies, clearAddMovie, setListMovieSort, setListMovieFilter, setListMovieView, setListMovieTableOption } from 'Store/Actions/addMovieActions';
import scrollPositions from 'Store/scrollPositions';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import withScrollPosition from 'Components/withScrollPosition';
import AddListMovie from './AddListMovie';

const POSTERS_PADDING = 15;
const POSTERS_PADDING_SMALL_SCREEN = 5;
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

  return scrollTop + padding;
}

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
    // Reset the scroll position before changing the view
    this.setState({ scrollTop: 0 }, () => {
      this.props.dispatchSetListMovieView(view);
    });
  }

  onScroll = ({ scrollTop }) => {
    this.setState({
      scrollTop
    }, () => {
      scrollPositions.addMovie = scrollTop;
    });
  }

  //
  // Render

  render() {
    return (
      <AddListMovie
        {...this.props}
        scrollTop={this.state.scrollTop}
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
  scrollTop: PropTypes.number.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchFetchListMovies: PropTypes.func.isRequired,
  dispatchClearListMovie: PropTypes.func.isRequired,
  dispatchSetListMovieView: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(AddDiscoverMovieConnector),
  'addMovie'
);
