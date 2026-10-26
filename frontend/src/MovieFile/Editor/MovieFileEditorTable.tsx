import React from 'react';
import MovieFileEditorTableContentConnector from './MovieFileEditorTableContentConnector';
import styles from './MovieFileEditorTable.css';

interface MovieFileEditorTableProps {
  movieId: number;
}

function MovieFileEditorTable(props: MovieFileEditorTableProps) {
  const { movieId } = props;

  return (
    <div className={styles.container}>
      <MovieFileEditorTableContentConnector movieId={movieId} />
    </div>
  );
}

export default MovieFileEditorTable;
