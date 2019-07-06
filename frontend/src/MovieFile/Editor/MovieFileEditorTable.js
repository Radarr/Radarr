import PropTypes from 'prop-types';
import React from 'react';
import MovieFileEditorTableContentConnector from './MovieFileEditorTableContentConnector';

function MovieFileEditorTable(props) {
  const {
    movieId
  } = props;

  return (
    <MovieFileEditorTableContentConnector
      movieId={movieId}
    />
  );
}

MovieFileEditorTable.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default MovieFileEditorTable;
