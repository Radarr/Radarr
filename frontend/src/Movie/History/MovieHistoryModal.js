import React from 'react';
import MovieHistoryModalContentConnector from './MovieHistoryModalContentConnector';

function MovieHistoryModal(props) {
  const {
    ...otherProps
  } = props;

  return (
      <MovieHistoryModalContentConnector
        {...otherProps}
      />
  );
}

MovieHistoryModal.propTypes = {
};

export default MovieHistoryModal;
