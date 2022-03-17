import PropTypes from 'prop-types';
import React from 'react';
import Slider from 'react-slick';
import styles from './Alert.css';

import 'slick-carousel/slick/slick.css';
import 'slick-carousel/slick/slick-theme.css';

function Carousel({ className, setRef, children, ...otherProps }) {

  const sliderSettings = {
    arrows: false,
    dots: false,
    infinite: false,
    slidesToShow: 1,
    slidesToScroll: 1,
    variableWidth: true
  };

  return (
    <Slider ref={setRef} {...sliderSettings}>
      {children}
    </Slider>
  );
}

Carousel.propTypes = {
  className: PropTypes.string.isRequired,
  setRef: PropTypes.func.isRequired,
  children: PropTypes.node.isRequired
};

Carousel.defaultProps = {
  className: styles.alert
};

export default Carousel;
