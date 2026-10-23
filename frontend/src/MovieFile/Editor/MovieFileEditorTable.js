import PropTypes from 'prop-types';
import React from 'react';
import MovieFileEditorTableContentConnector from './MovieFileEditorTableContentConnector';
import styles from './MovieFileEditorTable.css';

function MovieFileEditorTable(props) {
  const {
    movieId,
    versions,
    onRemoveVersionPress
  } = props;

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

MovieFileEditorTable.propTypes = {
  movieId: PropTypes.number.isRequired,
  versions: PropTypes.arrayOf(PropTypes.object),
  onRemoveVersionPress: PropTypes.func
};

export default MovieFileEditorTable;
