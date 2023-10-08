import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Navigation } from 'swiper';
import { Swiper, SwiperSlide } from 'swiper/react';
import dimensions from 'Styles/Variables/dimensions';
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
  }

  //
  // Control

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

  //
  // Render

  render() {
    const {
      items,
      itemComponent
    } = this.props;

    const {
      posterWidth,
      posterHeight,
      rowHeight
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
            <SwiperSlide key={credit.id} style={{ width: posterWidth, height: rowHeight }}>
              <MovieCreditPosterConnector
                key={credit.id}
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
