import PropTypes from 'prop-types';
import React from 'react';
import styles from './AddListMoviePosterInfo.css';

function AddListMoviePosterInfo(props) {
  const {
    studio,
    sortKey
  } = props;

  if (sortKey === 'studio' && studio) {
    return (
      <div className={styles.info}>
        {studio}
      </div>
    );
  }

  return null;
}

AddListMoviePosterInfo.propTypes = {
  studio: PropTypes.string,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default AddListMoviePosterInfo;
