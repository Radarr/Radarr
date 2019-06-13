import PropTypes from 'prop-types';
import React from 'react';
import SeasonInteractiveSearchModalContent from './SeasonInteractiveSearchModalContent';

function SeasonInteractiveSearchModal(props) {
  const {
    movieId
  } = props;

  return (
    <SeasonInteractiveSearchModalContent
      movieId={movieId}
    />
  );
}

SeasonInteractiveSearchModal.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default SeasonInteractiveSearchModal;
