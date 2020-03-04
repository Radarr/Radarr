import PropTypes from 'prop-types';
import React from 'react';
import MovieFileEditorTableContentConnector from './MovieFileEditorTableContentConnector';
import styles from './MovieFileEditorTable.css';

function MovieFileEditorTable(props) {
  const {
    movieId
  } = props;

  return (
    <div className={styles.container}>
      <MovieFileEditorTableContentConnector
        movieId={movieId}
      />
    </div>
  );
}

MovieFileEditorTable.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default MovieFileEditorTable;
