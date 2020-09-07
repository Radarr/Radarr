import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import styles from './StarRating.css';

function StarRating({ rating, votes, iconSize }) {
  const starWidth = {
    width: `${rating * 20}%`
  };

  const helpText = `${rating.toFixed(1)} (${votes} Votes)`;

  return (
    <span className={styles.starRating} title={helpText}>
      <div className={styles.backStar}>
        <Icon name={icons.STAR_FULL} size={iconSize} />
        <Icon name={icons.STAR_FULL} size={iconSize} />
        <Icon name={icons.STAR_FULL} size={iconSize} />
        <Icon name={icons.STAR_FULL} size={iconSize} />
        <Icon name={icons.STAR_FULL} size={iconSize} />
        <div className={styles.frontStar} style={starWidth}>
          <Icon name={icons.STAR_FULL} size={iconSize} />
          <Icon name={icons.STAR_FULL} size={iconSize} />
          <Icon name={icons.STAR_FULL} size={iconSize} />
          <Icon name={icons.STAR_FULL} size={iconSize} />
          <Icon name={icons.STAR_FULL} size={iconSize} />
        </div>
      </div>
    </span>
  );
}

StarRating.propTypes = {
  rating: PropTypes.number.isRequired,
  votes: PropTypes.number.isRequired,
  iconSize: PropTypes.number.isRequired
};

StarRating.defaultProps = {
  iconSize: 14
};

export default StarRating;
