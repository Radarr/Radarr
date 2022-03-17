import React from 'react';
import MovieTitlesTableContentConnector from './MovieTitlesTableContentConnector';
import styles from './MovieTitlesTable.css';

function MovieTitlesTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <div className={styles.container}>
      <MovieTitlesTableContentConnector
        {...otherProps}
      />
    </div>
  );
}

MovieTitlesTable.propTypes = {
};

export default MovieTitlesTable;
