import React, { useCallback, useMemo } from 'react';
import { Navigation } from 'swiper';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Swiper as SwiperClass } from 'swiper/types';
import dimensions from 'Styles/Variables/dimensions';
import MovieCredit from 'typings/MovieCredit';
import MovieCreditPoster from './MovieCreditPoster';
import styles from './MovieCreditPosters.css';

// Import Swiper styles
import 'swiper/css';
import 'swiper/css/navigation';

// Poster container dimensions
const columnPadding = parseInt(dimensions.movieIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(
  dimensions.movieIndexColumnPaddingSmallScreen
);

interface MovieCreditPostersProps {
  items: MovieCredit[];
  itemComponent: React.ElementType;
  isSmallScreen: boolean;
}

function MovieCreditPosters(props: MovieCreditPostersProps) {
  const { items, itemComponent, isSmallScreen } = props;

  const posterWidth = 162;
  const posterHeight = 238;

  const rowHeight = useMemo(() => {
    const titleHeight = 19;
    const characterHeight = 19;

    const heights = [
      posterHeight,
      titleHeight,
      characterHeight,
      isSmallScreen ? columnPaddingSmallScreen : columnPadding,
    ];

    return heights.reduce((acc, height) => acc + height, 0);
  }, [posterHeight, isSmallScreen]);

  const handleSwiperInit = useCallback((swiper: SwiperClass) => {
    swiper.navigation.init();
    swiper.navigation.update();
  }, []);

  return (
    <div className={styles.sliderContainer}>
      <Swiper
        slidesPerView="auto"
        spaceBetween={10}
        slidesPerGroup={isSmallScreen ? 1 : 3}
        navigation={true}
        loop={false}
        loopFillGroupWithBlank={true}
        className="mySwiper"
        modules={[Navigation]}
        onInit={handleSwiperInit}
      >
        {items.map((credit) => (
          <SwiperSlide
            key={credit.id}
            style={{ width: posterWidth, height: rowHeight }}
          >
            <MovieCreditPoster
              key={credit.id}
              component={itemComponent}
              posterWidth={posterWidth}
              posterHeight={posterHeight}
              tmdbId={credit.personTmdbId}
              personName={credit.personName}
              images={credit.images}
              job={credit.job}
              character={credit.character}
            />
          </SwiperSlide>
        ))}
      </Swiper>
    </div>
  );
}

export default MovieCreditPosters;
