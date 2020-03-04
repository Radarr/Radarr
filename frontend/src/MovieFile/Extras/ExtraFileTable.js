import PropTypes from 'prop-types';
import React from 'react';
import ExtraFileTableContentConnector from './ExtraFileTableContentConnector';
import styles from './ExtraFileTable.css';

function ExtraFileTable(props) {
  const {
    movieId
  } = props;

  return (
    <div className={styles.container}>
      <ExtraFileTableContentConnector
        movieId={movieId}
      />
    </div>

  );
}

ExtraFileTable.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default ExtraFileTable;
