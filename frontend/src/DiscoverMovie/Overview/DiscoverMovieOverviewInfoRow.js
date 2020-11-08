import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import styles from './DiscoverMovieOverviewInfoRow.css';

function DiscoverMovieOverviewInfoRow(props) {
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

DiscoverMovieOverviewInfoRow.propTypes = {
  title: PropTypes.string,
  iconName: PropTypes.object.isRequired,
  label: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired
};

export default DiscoverMovieOverviewInfoRow;
