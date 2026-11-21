import React from 'react';
import MovieVersion from 'Movie/Details/Versions/MovieVersion';
import MovieFileEditorTableContentConnector from './MovieFileEditorTableContentConnector';
import styles from './MovieFileEditorTable.css';

interface MovieFileEditorTableProps {
  movieId: number;
  versions?: MovieVersion[];
  onRemoveVersionPress?: (id: number) => void;
}

function MovieFileEditorTable(props: MovieFileEditorTableProps) {
  const { movieId, versions, onRemoveVersionPress } = props;

  return (
    <div className={styles.container}>
      <MovieFileEditorTableContentConnector
        movieId={movieId}
        versions={versions}
        onRemoveVersionPress={onRemoveVersionPress}
      />
    </div>
  );
}

export default MovieFileEditorTable;
