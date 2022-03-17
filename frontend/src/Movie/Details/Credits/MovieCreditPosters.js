import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Navigation } from 'swiper';
import { Swiper, SwiperSlide } from 'swiper/react';
import dimensions from 'Styles/Variables/dimensions';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import MovieCreditPosterConnector from './MovieCreditPosterConnector';
import styles from './MovieCreditPosters.css';

// Import Swiper styles
import 'swiper/css';
import 'swiper/css/navigation';

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

class MovieCreditPosters extends Component {

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
      items
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
            hasDifferentItemsOrOrder(prevProps.items, items))) {
      // recomputeGridSize also forces Grid to discard its cache of rendered cells
      this._grid.recomputeGridSize();
    }
  }

  //
  // Control

  setGridRef = (ref) => {
    this._grid = ref;
  };

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
  };

  cellRenderer = ({ key, rowIndex, columnIndex, style }) => {
    const {
      items,
      itemComponent
    } = this.props;

    const {
      posterWidth,
      posterHeight,
      columnCount
    } = this.state;

    const movieIdx = rowIndex * columnCount + columnIndex;
    const movie = items[movieIdx];

    if (!movie) {
      return null;
    }

    return (
      <div
        className={styles.container}
        key={key}
        style={style}
      >
        <MovieCreditPosterConnector
          key={movie.order}
          component={itemComponent}
          posterWidth={posterWidth}
          posterHeight={posterHeight}
          tmdbId={movie.personTmdbId}
          personName={movie.personName}
          job={movie.job}
          character={movie.character}
          images={movie.images}
        />
      </div>
    );
  };

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.calculateGrid(width, this.props.isSmallScreen);
  };

  //
  // Render

  render() {
    const {
      items,
      itemComponent
    } = this.props;

    const {
      posterWidth,
      posterHeight
    } = this.state;

    return (

      <div className={styles.sliderContainer}>
        <Swiper
          slidesPerView='auto'
          spaceBetween={10}
          slidesPerGroup={3}
          loop={false}
          loopFillGroupWithBlank={true}
          className="mySwiper"
          modules={[Navigation]}
          onInit={(swiper) => {
            swiper.params.navigation.prevEl = this._swiperPrevRef;
            swiper.params.navigation.nextEl = this._swiperNextRef;
            swiper.navigation.init();
            swiper.navigation.update();
          }}
        >
          {items.map((credit) => (
            <SwiperSlide key={credit.tmdbId} style={{ width: posterWidth }}>
              <MovieCreditPosterConnector
                key={credit.order}
                component={itemComponent}
                posterWidth={posterWidth}
                posterHeight={posterHeight}
                tmdbId={credit.personTmdbId}
                personName={credit.personName}
                job={credit.job}
                character={credit.character}
                images={credit.images}
              />
            </SwiperSlide>
          ))}
        </Swiper>
      </div>
    );
  }
}

MovieCreditPosters.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  itemComponent: PropTypes.elementType.isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default MovieCreditPosters;
