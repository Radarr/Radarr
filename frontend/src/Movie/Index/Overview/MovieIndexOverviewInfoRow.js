import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import styles from './MovieIndexOverviewInfoRow.css';

function MovieIndexOverviewInfoRow(props) {
  const {
    title,
    iconName,
    label
  } = props;

  return (
    <div
      className={styles.infoRow}
      title={title}
    >
      <Icon
        className={styles.icon}
        name={iconName}
        size={14}
      />

      {label}
    </div>
  );
}

MovieIndexOverviewInfoRow.propTypes = {
  title: PropTypes.string,
  iconName: PropTypes.object.isRequired,
  label: PropTypes.string.isRequired
};

export default MovieIndexOverviewInfoRow;
