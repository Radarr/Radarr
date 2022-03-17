import React from 'react';
import MovieHistoryTableContentConnector from './MovieHistoryTableContentConnector';
import styles from './MovieHistoryTable.css';

function MovieHistoryTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <div className={styles.container}>
      <MovieHistoryTableContentConnector
        {...otherProps}
      />
    </div>
  );
}

MovieHistoryTable.propTypes = {
};

export default MovieHistoryTable;
