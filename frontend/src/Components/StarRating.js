import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import styles from './StarRating.css';

function StarRating({ rating, votes, iconSize }) {
  const starWidth = {
    width: `${rating * 10}%`
  };

  const helpText = `${rating/2} (${votes} Votes)`;

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
