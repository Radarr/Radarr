import React from 'react';
import MovieTitlesTableContentConnector from './MovieTitlesTableContentConnector';

function MovieTitlesTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <MovieTitlesTableContentConnector
      {...otherProps}
    />
  );
}

MovieTitlesTable.propTypes = {
};

export default MovieTitlesTable;
