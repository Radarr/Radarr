import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieClientSideCollectionItemsSelector from 'Store/Selectors/createMovieClientSideCollectionItemsSelector';
import dimensions from 'Styles/Variables/dimensions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { fetchMovies } from 'Store/Actions/movieActions';
import scrollPositions from 'Store/scrollPositions';
import { setMovieSort, setMovieFilter, setMovieView } from 'Store/Actions/movieIndexActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import MovieIndex from './MovieIndex';

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
    createMovieClientSideCollectionItemsSelector('movieIndex'),
    createCommandExecutingSelector(commandNames.REFRESH_MOVIE),
    createCommandExecutingSelector(commandNames.RSS_SYNC),
    createDimensionsSelector(),
    (
      movies,
      isRefreshingMovie,
      isRssSyncExecuting,
      dimensionsState
    ) => {
      return {
        ...movies,
        isRefreshingMovie,
        isRssSyncExecuting,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

const mapDispatchToProps = {
  fetchMovies,
  setMovieSort,
  setMovieFilter,
  setMovieView,
  executeCommand
};

class MovieIndexConnector extends Component {

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
    this.props.fetchMovies();
  }

  //
  // Listeners

  onSortSelect = (sortKey) => {
    this.props.setMovieSort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.setMovieFilter({ selectedFilterKey });
  }

  onViewSelect = (view) => {
    // Reset the scroll position before changing the view
    this.setState({ scrollTop: 0 }, () => {
      this.props.setMovieView({ view });
    });
  }

  onScroll = ({ scrollTop }) => {
    this.setState({
      scrollTop
    }, () => {
      scrollPositions.movieIndex = scrollTop;
    });
  }

  onRefreshMoviePress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_MOVIE
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
      <MovieIndex
        {...this.props}
        scrollTop={this.state.scrollTop}
        onSortSelect={this.onSortSelect}
        onFilterSelect={this.onFilterSelect}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
        onRefreshMoviePress={this.onRefreshMoviePress}
        onRssSyncPress={this.onRssSyncPress}
      />
    );
  }
}

MovieIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  scrollTop: PropTypes.number.isRequired,
  fetchMovies: PropTypes.func.isRequired,
  setMovieSort: PropTypes.func.isRequired,
  setMovieFilter: PropTypes.func.isRequired,
  setMovieView: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, mapDispatchToProps)(MovieIndexConnector),
  'movieIndex'
);
