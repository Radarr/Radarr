import PropTypes from 'prop-types';
import React from 'react';
import InteractiveSearchTableContent from './InteractiveSearchTableContent';

function InteractiveSearchTable(props) {
  const {
    movieId
  } = props;

  return (
    <InteractiveSearchTableContent
      movieId={movieId}
    />
  );
}

InteractiveSearchTable.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default InteractiveSearchTable;
