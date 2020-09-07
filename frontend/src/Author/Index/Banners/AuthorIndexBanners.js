import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Grid, WindowScroller } from 'react-virtualized';
import AuthorIndexItemConnector from 'Author/Index/AuthorIndexItemConnector';
import Measure from 'Components/Measure';
import dimensions from 'Styles/Variables/dimensions';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import AuthorIndexBanner from './AuthorIndexBanner';
import styles from './AuthorIndexBanners.css';

//  container dimensions
const columnPadding = parseInt(dimensions.authorIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.authorIndexColumnPaddingSmallScreen);
const progressBarHeight = parseInt(dimensions.progressBarSmallHeight);
const detailedProgressBarHeight = parseInt(dimensions.progressBarMediumHeight);

const additionalColumnCount = {
  small: 3,
  medium: 2,
  large: 1
};

function calculateColumnWidth(width, bannerSize, isSmallScreen) {
  const maxiumColumnWidth = isSmallScreen ? 344 : 364;
  const columns = Math.floor(width / maxiumColumnWidth);
  const remainder = width % maxiumColumnWidth;

  if (remainder === 0 && bannerSize === 'large') {
    return maxiumColumnWidth;
  }

  return Math.floor(width / (columns + additionalColumnCount[bannerSize]));
}

function calculateRowHeight(bannerHeight, sortKey, isSmallScreen, bannerOptions) {
  const {
    detailedProgressBar,
    showTitle,
    showMonitored,
    showQualityProfile
  } = bannerOptions;

  const nextAiringHeight = 19;

  const heights = [
    bannerHeight,
    detailedProgressBar ? detailedProgressBarHeight : progressBarHeight,
    nextAiringHeight,
    isSmallScreen ? columnPaddingSmallScreen : columnPadding
  ];

  if (showTitle) {
    heights.push(19);
  }

  if (showMonitored) {
    heights.push(19);
  }

  if (showQualityProfile) {
    heights.push(19);
  }

  switch (sortKey) {
    case 'seasons':
    case 'previousAiring':
    case 'added':
    case 'path':
    case 'sizeOnDisk':
      heights.push(19);
      break;
    case 'qualityProfileId':
      if (!showQualityProfile) {
        heights.push(19);
      }
      break;
    default:
      // No need to add a height of 0
  }

  return heights.reduce((acc, height) => acc + height, 0);
}

function calculateHeight(bannerWidth) {
  return Math.ceil((88/476) * bannerWidth);
}

class AuthorIndexBanners extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      width: 0,
      columnWidth: 364,
      columnCount: 1,
      bannerWidth: 476,
      bannerHeight: 88,
      rowHeight: calculateRowHeight(88, null, props.isSmallScreen, {})
    };

    this._isInitialized = false;
    this._grid = null;
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      items,
      sortKey,
      bannerOptions,
      jumpToCharacter
    } = this.props;

    const {
      width,
      columnWidth,
      columnCount,
      rowHeight
    } = this.state;

    if (prevProps.sortKey !== sortKey ||
        prevProps.bannerOptions !== bannerOptions) {
      this.calculateGrid();
    }

    if (this._grid &&
        (prevState.width !== width ||
            prevState.columnWidth !== columnWidth ||
            prevState.columnCount !== columnCount ||
            prevState.rowHeight !== rowHeight ||
            hasDifferentItemsOrOrder(prevProps.items, items))) {
      // recomputeGridSize also forces Grid to discard its cache of rendered cells
      this._grid.recomputeGridSize();
    }

    if (jumpToCharacter != null && jumpToCharacter !== prevProps.jumpToCharacter) {
      const index = getIndexOfFirstCharacter(items, jumpToCharacter);

      if (this._grid && index != null) {
        const row = Math.floor(index / columnCount);

        this._grid.scrollToCell({
          rowIndex: row,
          columnIndex: 0
        });
      }
    }
  }

  //
  // Control

  setGridRef = (ref) => {
    this._grid = ref;
  }

  calculateGrid = (width = this.state.width, isSmallScreen) => {
    const {
      sortKey,
      bannerOptions
    } = this.props;

    const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;
    const columnWidth = calculateColumnWidth(width, bannerOptions.size, isSmallScreen);
    const columnCount = Math.max(Math.floor(width / columnWidth), 1);
    const bannerWidth = columnWidth - padding;
    const bannerHeight = calculateHeight(bannerWidth);
    const rowHeight = calculateRowHeight(bannerHeight, sortKey, isSmallScreen, bannerOptions);

    this.setState({
      width,
      columnWidth,
      columnCount,
      bannerWidth,
      bannerHeight,
      rowHeight
    });
  }

  cellRenderer = ({ key, rowIndex, columnIndex, style }) => {
    const {
      items,
      sortKey,
      bannerOptions,
      showRelativeDates,
      shortDateFormat,
      timeFormat
    } = this.props;

    const {
      bannerWidth,
      bannerHeight,
      columnCount
    } = this.state;

    const {
      detailedProgressBar,
      showTitle,
      showMonitored,
      showQualityProfile
    } = bannerOptions;

    const author = items[rowIndex * columnCount + columnIndex];

    if (!author) {
      return null;
    }

    return (
      <div
        style={style}
        key={key}
      >
        <AuthorIndexItemConnector
          key={author.id}
          component={AuthorIndexBanner}
          sortKey={sortKey}
          bannerWidth={bannerWidth}
          bannerHeight={bannerHeight}
          detailedProgressBar={detailedProgressBar}
          showTitle={showTitle}
          showMonitored={showMonitored}
          showQualityProfile={showQualityProfile}
          showRelativeDates={showRelativeDates}
          shortDateFormat={shortDateFormat}
          timeFormat={timeFormat}
          authorId={author.id}
          qualityProfileId={author.qualityProfileId}
          metadataProfileId={author.metadataProfileId}
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
      items,
      isSmallScreen,
      scroller
    } = this.props;

    const {
      width,
      columnWidth,
      columnCount,
      rowHeight
    } = this.state;

    const rowCount = Math.ceil(items.length / columnCount);

    return (
      <Measure
        whitelist={['width']}
        onMeasure={this.onMeasure}
      >
        <WindowScroller
          scrollElement={isSmallScreen ? undefined : scroller}
        >
          {({ height, registerChild, onChildScroll, scrollTop }) => {
            if (!height) {
              return <div />;
            }

            return (
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
            );
          }
          }
        </WindowScroller>
      </Measure>
    );
  }
}

AuthorIndexBanners.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  bannerOptions: PropTypes.object.isRequired,
  jumpToCharacter: PropTypes.string,
  scroller: PropTypes.instanceOf(Element).isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default AuthorIndexBanners;
