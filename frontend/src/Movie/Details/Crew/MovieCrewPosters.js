import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Grid, WindowScroller } from 'react-virtualized';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import dimensions from 'Styles/Variables/dimensions';
import Measure from 'Components/Measure';
import MovieCrewPoster from './MovieCrewPoster';
import styles from './MovieCrewPosters.css';

// Poster container dimensions
const columnPadding = parseInt(dimensions.movieIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.movieIndexColumnPaddingSmallScreen);

const additionalColumnCount = {
  small: 3,
  medium: 2,
  large: 1
};

function calculateColumnWidth(width, posterSize, isSmallScreen) {
  const maxiumColumnWidth = isSmallScreen ? 172 : 182;
  const columns = Math.floor(width / maxiumColumnWidth);
  const remainder = width % maxiumColumnWidth;

  if (remainder === 0 && posterSize === 'large') {
    return maxiumColumnWidth;
  }

  return Math.floor(width / (columns + additionalColumnCount[posterSize]));
}

function calculateRowHeight(posterHeight, isSmallScreen) {
  const titleHeight = 19;
  const characterHeight = 19;

  const heights = [
    posterHeight,
    titleHeight,
    characterHeight,
    isSmallScreen ? columnPaddingSmallScreen : columnPadding
  ];

  return heights.reduce((acc, height) => acc + height, 0);
}

function calculatePosterHeight(posterWidth) {
  return Math.ceil((250 / 170) * posterWidth);
}

class MovieCrewPosters extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      width: 0,
      columnWidth: 182,
      columnCount: 1,
      posterWidth: 162,
      posterHeight: 238,
      rowHeight: calculateRowHeight(238, props.isSmallScreen)
    };

    this._isInitialized = false;
    this._grid = null;
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      crew
    } = this.props;

    const {
      width,
      columnWidth,
      columnCount,
      rowHeight
    } = this.state;

    if (this._grid &&
        (prevState.width !== width ||
            prevState.columnWidth !== columnWidth ||
            prevState.columnCount !== columnCount ||
            prevState.rowHeight !== rowHeight ||
            hasDifferentItemsOrOrder(prevProps.crew, crew))) {
      // recomputeGridSize also forces Grid to discard its cache of rendered cells
      this._grid.recomputeGridSize();
    }
  }

  //
  // Control

  setGridRef = (ref) => {
    this._grid = ref;
  }

  calculateGrid = (width = this.state.width, isSmallScreen) => {

    const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;
    const columnWidth = calculateColumnWidth(width, 'small', isSmallScreen);
    const columnCount = Math.max(Math.floor(width / columnWidth), 1);
    const posterWidth = columnWidth - padding;
    const posterHeight = calculatePosterHeight(posterWidth);
    const rowHeight = calculateRowHeight(posterHeight, isSmallScreen);

    this.setState({
      width,
      columnWidth,
      columnCount,
      posterWidth,
      posterHeight,
      rowHeight
    });
  }

  cellRenderer = ({ key, rowIndex, columnIndex, style }) => {
    const {
      crew
    } = this.props;

    const {
      posterWidth,
      posterHeight,
      columnCount
    } = this.state;

    const movieIdx = rowIndex * columnCount + columnIndex;
    const movie = crew[movieIdx];

    if (!movie) {
      return null;
    }

    return (
      <div
        className={styles.container}
        key={key}
        style={style}
      >
        <MovieCrewPoster
          key={movie.order}
          posterWidth={posterWidth}
          posterHeight={posterHeight}
          crewId={movie.tmdbId}
          crewName={movie.name}
          job={movie.job}
          images={movie.images}
        />
      </div>
    );
  }

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.calculateGrid(width, this.props.isSmallScreen);
  }

  //
  // Render

  render() {
    const {
      crew
    } = this.props;

    const {
      width,
      columnWidth,
      columnCount,
      rowHeight
    } = this.state;

    const rowCount = Math.ceil(crew.length / columnCount);

    return (
      <Measure
        whitelist={['width']}
        onMeasure={this.onMeasure}
      >
        <WindowScroller
          scrollElement={undefined}
        >
          {({ height, registerChild, onChildScroll, scrollTop }) => {
            if (!height) {
              return <div />;
            }

            return (
              <div ref={registerChild}>
                <Grid
                  ref={this.setGridRef}
                  className={styles.grid}
                  autoHeight={true}
                  height={height}
                  columnCount={columnCount}
                  columnWidth={columnWidth}
                  rowCount={rowCount}
                  rowHeight={rowHeight}
                  width={width}
                  onScroll={onChildScroll}
                  scrollTop={scrollTop}
                  overscanRowCount={2}
                  cellRenderer={this.cellRenderer}
                  scrollToAlignment={'start'}
                  isScrollingOptOut={true}
                />
              </div>
            );
          }
          }
        </WindowScroller>
      </Measure>
    );
  }
}

MovieCrewPosters.propTypes = {
  crew: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default MovieCrewPosters;
