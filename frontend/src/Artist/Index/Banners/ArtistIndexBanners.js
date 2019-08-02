import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import { Grid, WindowScroller } from 'react-virtualized';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import dimensions from 'Styles/Variables/dimensions';
import { sortDirections } from 'Helpers/Props';
import Measure from 'Components/Measure';
import ArtistIndexItemConnector from 'Artist/Index/ArtistIndexItemConnector';
import ArtistIndexBanner from './ArtistIndexBanner';
import styles from './ArtistIndexBanners.css';

//  container dimensions
const columnPadding = parseInt(dimensions.artistIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.artistIndexColumnPaddingSmallScreen);
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

class ArtistIndexBanners extends Component {

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

  componentDidMount() {
    this._contentBodyNode = ReactDOM.findDOMNode(this.props.contentBody);
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      filters,
      sortKey,
      sortDirection,
      bannerOptions,
      jumpToCharacter
    } = this.props;

    const itemsChanged = hasDifferentItems(prevProps.items, items);

    if (
      prevProps.sortKey !== sortKey ||
      prevProps.bannerOptions !== bannerOptions ||
      itemsChanged
    ) {
      this.calculateGrid();
    }

    if (
      prevProps.filters !== filters ||
      prevProps.sortKey !== sortKey ||
      prevProps.sortDirection !== sortDirection ||
      itemsChanged
    ) {
      this._grid.recomputeGridSize();
    }

    if (jumpToCharacter != null && jumpToCharacter !== prevProps.jumpToCharacter) {
      const index = getIndexOfFirstCharacter(items, jumpToCharacter);

      if (index != null) {
        const {
          columnCount,
          rowHeight
        } = this.state;

        const row = Math.floor(index / columnCount);
        const scrollTop = rowHeight * row;

        this.props.onScroll({ scrollTop });
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

    const artist = items[rowIndex * columnCount + columnIndex];

    if (!artist) {
      return null;
    }

    return (
      <ArtistIndexItemConnector
        key={key}
        component={ArtistIndexBanner}
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
        style={style}
        artistId={artist.id}
        qualityProfileId={artist.qualityProfileId}
        metadataProfileId={artist.metadataProfileId}
      />
    );
  }

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.calculateGrid(width, this.props.isSmallScreen);
  }

  onSectionRendered = () => {
    if (!this._isInitialized && this._contentBodyNode) {
      this.props.onRender();
      this._isInitialized = true;
    }
  }

  //
  // Render

  render() {
    const {
      items,
      scrollTop,
      isSmallScreen,
      onScroll
    } = this.props;

    const {
      width,
      columnWidth,
      columnCount,
      rowHeight
    } = this.state;

    const rowCount = Math.ceil(items.length / columnCount);

    return (
      <Measure onMeasure={this.onMeasure}>
        <WindowScroller
          scrollElement={isSmallScreen ? undefined : this._contentBodyNode}
          onScroll={onScroll}
        >
          {({ height, isScrolling }) => {
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
                scrollTop={scrollTop}
                overscanRowCount={2}
                cellRenderer={this.cellRenderer}
                onSectionRendered={this.onSectionRendered}
              />
            );
          }
          }
        </WindowScroller>
      </Measure>
    );
  }
}

ArtistIndexBanners.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  bannerOptions: PropTypes.object.isRequired,
  scrollTop: PropTypes.number.isRequired,
  jumpToCharacter: PropTypes.string,
  contentBody: PropTypes.object.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onRender: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default ArtistIndexBanners;
