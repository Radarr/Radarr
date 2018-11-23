import PropTypes from 'prop-types';
import React from 'react';
import styles from './MovieAlternateTitles.css';

function MovieAlternateTitles({ alternateTitles }) {
  return (
    <ul>
      {
        alternateTitles.map((alternateTitle) => {
          return (
            <li
              key={alternateTitle}
              className={styles.alternateTitle}
            >
              {alternateTitle}
            </li>
          );
        })
      }
    </ul>
  );
}

MovieAlternateTitles.propTypes = {
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired
};

export default MovieAlternateTitles;
