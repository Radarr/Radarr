import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import MovieInteractiveSearchModalContent from './MovieInteractiveSearchModalContent';

function MovieInteractiveSearchModal(props) {
  const {
    isOpen,
    movieId,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
      size={sizes.EXTRA_EXTRA_LARGE}
    >
      <MovieInteractiveSearchModalContent
        movieId={movieId}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

MovieInteractiveSearchModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  movieId: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MovieInteractiveSearchModal;
