import React from 'react';
import MovieHistoryTableContentConnector from './MovieHistoryTableContentConnector';

function MovieHistoryTable(props) {
  const {
    ...otherProps
  } = props;

  return (
      <MovieHistoryTableContentConnector
        {...otherProps}
      />
  );
}

MovieHistoryTable.propTypes = {
};

export default MovieHistoryTable;
